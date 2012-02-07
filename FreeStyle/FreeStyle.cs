using Sifteo;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FreeStyle
{
  public enum PhaseType {MODESELECT, NAMESELECT, BEATSELECT, BEATSELECT2, THEMESELECT, RAPSTART1, RAPSTART2, RAPSTART3, RAPSTART4, RAPSTART5, LYRICS, SCORES, ENDOFRAP, TRIVIA1, TRIVIA2, TRIVIA3, DOUBLEDIS};
  public enum CubeType {SELECTOR, SELECTABLE, NONE};
  public enum CubeName {DEFENDER, CHALLENGER, MIC, DIS, JUSTDISSED, QUESTION, ANSWER, NAMES1, NAMES2, BEAT1, BEAT2, THEMES1, THEMES2, SELECTOR}
  public enum PlayerNames {JO = 0, FLO = 1, BO = 2, BRO = 3, R2D2 = 4, CHUMP = 5, RADTAD = 6, STU = 7};
  public enum Themes {BOAST = 0, SOCIETY = 1, ROAST = 2, LOVE = 3, ANIMALS = 4, CHILDHOOD = 5, GEOGRAPHY = 6, POLITICS = 7};
  public enum DisplayType {TEXT, IMAGE};
  public enum GameModes {FREELIB = 0, FREETHEME = 1}
	
  public class FreeStyleApp : BaseApp
  {
	public PhaseType currentPhase = PhaseType.MODESELECT;
	public PlayerNames currentPlayer;
	public GameModes currentMode;
	public bool readyToChangeState = false;
	public int ticks = 0;
	public Sound mMusic;
	public Sound sampleMusic;
	public Sound fxMusic;
	private int triviaIndex = 0;
	private string[] triviaQuestions = {"TRUE OR,FALSE?,HUMANS EVOLVED FROM LIZARDS", "AKON'S,FATHER,IS FROM,WHAT,COUNTRY?", "WHO IS,JLO'S CURRENT,SPOUSE?","PANASONIC,IS BASED,IN WHICH,COUNTRY?","WHAT IS,NEWT,GINGRICH'S,GIVEN FIRST,NAME?","NAME 2,OF THE,3 MOST POPULAR,WEBSITES,EXCL GOOGLE","WHY ARE,YOU HOT?","WHERE DID,PUFF DADDY,GET HIS,NICKNAME?"};
	private string[] triviaAnswers = {"TRUE","SENEGAL","NO SPOUSE","JAPAN","NEWTON","FACEBOOK,YOUTUBE,YAHOO,LIVE.COM","I'M HOT,CAUSE I'M,FLY","HE WOULD,HUFF AND,PUFF WHEN,ANGRY"};
	public CubeName buzzer;
	public Themes currentTheme;
	public int currentVerseIndex = 0;
	public string[] verseLines;
	public int ticksWaiting = 0;
	public int[] scores = {-1,-1,-1,-1,-1,-1,-1,-1};
    public List<CubeWrapper> mWrappers = new List<CubeWrapper>();
    Cube[] currentCubes;
 
		
    override public int FrameRate
    {
      get { return 20; }
    }

    // called during intitialization, before the game has started to run
    override public void Setup()
    {
      Log.Debug("Setup()");
			
	  currentCubes = this.CubeSet.toArray();
	  currentVerseIndex = 0;
	  mMusic = Sounds.CreateSound("intro");
	  mMusic.Play(1, -1);
			
	  CubeWrapper wrapper1 = new CubeWrapper(this, currentCubes[0]);
      wrapper1.mImage = "freeStyle";
	  wrapper1.textOrImage = DisplayType.IMAGE;
	  mWrappers.Add(wrapper1);		
			
	  CubeWrapper wrapper2 = new CubeWrapper(this, currentCubes[1]);
	  wrapper2.mImage = "modeSelect";
	  wrapper2.textOrImage = DisplayType.IMAGE;
	  wrapper2.mType = CubeType.SELECTABLE;
	  mWrappers.Add(wrapper2);		
			
	  CubeWrapper wrapper3 = new CubeWrapper(this, currentCubes[2]);		
	  wrapper3.mImage = "selector";
	  wrapper3.textOrImage = DisplayType.IMAGE;
	  wrapper3.mType = CubeType.SELECTOR;
	  mWrappers.Add(wrapper3);
			
      foreach (CubeWrapper wrapper in mWrappers) {
        wrapper.DrawSlide();
	  }
	
	  
			
	  CubeSet.NeighborAddEvent += OnNeighborAdd;
      CubeSet.NeighborRemoveEvent += OnNeighborRemove;
			
			
    }
    
    private void OnNeighborAdd(Cube cube1, Cube.Side side1, Cube cube2, Cube.Side side2)  {
      Log.Debug("Neighbor add: {0}.{1} <-> {2}.{3}", cube1.UniqueId, side1, cube2.UniqueId, side2);

      CubeWrapper wrapper1 = (CubeWrapper)cube1.userData;
	  CubeWrapper wrapper2 = (CubeWrapper)cube2.userData;		
			
      if (wrapper1 != null && wrapper2 != null) {
        if ((wrapper1.mType == CubeType.SELECTOR && wrapper2.mType == CubeType.SELECTABLE) || (wrapper2.mType == CubeType.SELECTOR && wrapper1.mType == CubeType.SELECTABLE)) {
		  Cube.Side selectorSide;
		  Cube.Side selectableSide;
		  CubeWrapper selectorWrapper;
		  CubeWrapper selectableWrapper;
		  
		  if (wrapper1.mType == CubeType.SELECTOR) {
			selectorSide = side1;	
			selectableSide = side2;
			selectorWrapper = wrapper1;
			selectableWrapper = wrapper2;
		  }				
		  
		  else {
			selectorSide = side2;
			selectableSide = side1;
			selectorWrapper = wrapper2;
			selectableWrapper = wrapper1;
		  }
					
		  if (selectorSide == Cube.Side.BOTTOM) {
			switch (currentPhase) {
						case (PhaseType.MODESELECT):
							if (selectableSide == Cube.Side.TOP) {
								currentMode = GameModes.FREELIB;
								currentPhase = PhaseType.NAMESELECT;
							    readyToChangeState = true;
							}
							else if (selectableSide == Cube.Side.BOTTOM) {
								currentMode = GameModes.FREETHEME;
								currentPhase = PhaseType.NAMESELECT;
							    readyToChangeState = true;
							}
							
							break;
							
						case (PhaseType.NAMESELECT):

							currentPlayer = nameSelector(selectableSide, selectableWrapper);
							currentPhase = PhaseType.THEMESELECT;
							readyToChangeState = true;
							break;
							
						case (PhaseType.BEATSELECT):
							
							string sample = beatSelector(selectableSide, selectableWrapper);
							sampleMusic = Sounds.CreateSound(sample);
							
							if (mMusic.IsPlaying) mMusic.Pause();
							
							sampleMusic.Play(1, -1);
							currentPhase = PhaseType.BEATSELECT2;
							readyToChangeState = true;
							break;
							
						case (PhaseType.THEMESELECT):

							currentTheme = themeSelector(selectableSide, selectableWrapper);
							currentPhase = PhaseType.BEATSELECT;
							readyToChangeState = true;
							break;
							
						default:
							break;
							
			}
	 	  }		
		}
      }
    }
	
	private string[] initializeVerseLines() {
		
		if (currentMode == GameModes.FREELIB) {
				
			switch (currentTheme) {
					
				case Themes.ANIMALS:
					return new string[] {"AMINALS"};

				case Themes.BOAST:
					
					return new string[] {"MY NAME IS,__AND IM,HERE TO,SAY", "I LIKE TO,__AND I,LIKE TO,PLAY", "MY RHYMES,ARE__,AND MY,RHYMES ARE,SICK", "AND IM THE,__ OF MY,CLIQUE", "I CAME,FROM __,NOW IM AT,THE TOP", "I ONLY __,I NEVER, FLOP", "IM SO,WEALTHY I,______,FOR DINNER", "AND MY,LOVER YEAH,IS A __","IM A,GOLDEN,CHILD","IM TOO __,TO HANDLE", "IM __ THAN,A SHARK","IM __,THAN A,CANDLE", "IM A __,IN BED,AND YOU,KNOW IM,RIGHT","AND I __,YOUR MOMMA,LAST NITE","WHEN I OPEN,MY __,MY __,GOES __", "AND THAT,IS WHY,I AM,SO __","SO ___,WITHOUT A,DOUBT","AND WITH,THAT IM,OUT"};
				
				case Themes.CHILDHOOD:
					
					return new string[] {"CHILDHOOD"};

				case Themes.GEOGRAPHY:
					
					return new string[] {"GEO"};

				case Themes.LOVE:
					
					return new string[] {"IM SO,IN LOVE,IT MAKES,ME DIZZY","JUST ___,ABOUT YOU,KEEPS ME,BUSY","IT WAS,SO ___,WHEN WE,MET","IT WAS,THE ___,DAY YET","YOUR LAUGH,WAS SO CUTE,YOUR ___,WERE IN,STYLE","I JUST,WANTED,TO ___,FOR A,WHILE","I FEEL,LIKE A,___ WHEN,YOURE AROUND","I ONCE,WAS __,AND NOW,IM ___","YOU MAKE,___ GOOD","AND THATS,JUST ___","THAT WHY,______,______","LETS ___,LETS ___,LETS HAVE,SOME FUN","LETS ___,UNTIL WE,SEE THE,SUN","YOU MY,DEAR,ARE NUMBER __","MY LIFE,WOULD BE,SO ___,ALONE","I NEED ___","LIKE A, ___ NEEDS,A BONE","I WANT,TO __","YOUR TOE,NAILS","AND I,KNOW OUR,___","WILL NEVER,FAIL"};

				case Themes.POLITICS:
					
					return new string[] {"WHY DIVIDE,WHEN WE,COULD UNITE","WHY ALL,THIS ___,WHY DO WE,FIGHT","COME ON,PEOPLE,LETS GET,TOGETHER","SEEDS OF,___ ARE,BLOWIN IN,THE WEATHER","ALL WE,NEED IS,___,AND MAYBE ___,TOO","LETS MAKE,THE GOVERN,MENT ___,FOR YOU","LETS ___,THE NEEDY,AND GIVE,THEM ___","LETS __,THE RICH,AND ____,THEIR ___","WHO NEEDS,____,WHEN WE,HAVE TWITTER","WHY ARE WE,___ WHY ARE,WE BITTER","IF I WERE,IN CHARGE,ID ___,ALL DAY","ID CREATE,A ____,SO PEOPLE,COULD __","ID BUILD,A ___,ID GO TO CHINA","ID ____,IN CAROLINA","ID VOTE FOR, _____,BUT NOT FOR,_____","EVERYONE,WOULD SAY,_________,_________","SO PLEASE, DONT LET,THE BAD,GUYS HUSH,YOU","VOTE FOR,ME OR, I WILL,___ YOU"}; 
						

				case Themes.ROAST:
					
					return new string[] {"THIS IS,FOR THAT,PERSON WHO,MADE ME,SAD","WHO STOLE,FROM ME,THE LITTLE,THAT I HAD,","YOURE A __,I SAY,AND THATS,NO GOOD","YOURE THE,__ PERSON,IN OUR,NEIGHBOR,HOOD","YOURE REALLY,__ THATS,WHAT I,THINK","EXCUSE MY,__ BUT ID,SAY YOU,STINK","YOURE SUCH,A ___,ALWAYS ___,ALL AROUND,","YOU MAKE,A __,WIHOUT,MAKING,A SOUND,", "YOUR BREATH,IS LIKE,__","YOU __ LIKE A GOAT","YOUR TEETH,ARE ALL,__","YOU NEED,TO EAT,MORE OATS","YOUR LAUGH,IS LIKE __,IT MAKES ME,___","AND THATS,WHY I SAY,DONT ______","YOU DONT,EVEN KNOW,HOW TO __","SO GO AWAY,__ BEFORE,I ___","YOULL NEVER,________,AND YOULL,NEVER BE,COOL","YOULL ALWAYS,BE A ___"};

				case Themes.SOCIETY:
					
					return new string[] {"SOCIETY"};

				default:
					
					break;
					
				}
			}	
			else if (currentMode == GameModes.FREETHEME) {
		      
			  switch (currentTheme) {  		

				case Themes.ANIMALS:
					return new string[] {"RABBIT","HOLE","CARROT","ANT","EAT","ALL THE WORLD","DINOSAURS","CONDOR","WINGS","THINGS","SWARM","WARM","WHALE","FAIL","TAIL","TURTLE","SNAKE","SERPENT","DOG EAT DOG","ANOTHER WORLD","BACTERIA","BEES","FLEAS","CHEESE","EASE","SEIZE","VULTURE","CULTURE","SYSTEM", "RHYTHM","WALRUS","TALLEST"};

				case Themes.BOAST:
					
					return new string[] {"MASTER","MOST BEST","QUARTER,BACK","GOBLET","KING","BANK,ACCOUNT","OLD,MACDONALD","REAL MCCOY","WOLF","CHAMP","SHINY","HELICOPTER","GREATEST","PRIVATE,YACHT","BEACH,PARTY","IM SO,SMART","TOP OF,THE HEAP","THE ONE,AND ONLY","CHOSEN","HERO","DESTINY","RICHES","THANKS","THE BEST,THERE,EVER WAS"};
	
				case Themes.CHILDHOOD:
					
					return new string[] {"CHILDHOOD"};

				case Themes.GEOGRAPHY:
					
					return new string[] {"GEO"};

				case Themes.LOVE:
					
					return new string[] {"LOVE"};

				case Themes.POLITICS:
					
					return new string[] {"POLITICS"};

				case Themes.ROAST:
					
					return new string[] {"FLAMES","ROAST","WOLF","NUTS","TRICK OR,TREAT","SORRY","SNACK ON,THIS","CRUMBLE","SNAKEBITE","GYPSY KING","GRIMIEST","GROUCH","LIKE A,DOPE","NINJA","BODY SLAM","CHUMP","RUSTY","FLAKY","BABY FOOD","FLOWER","WHISPER","GOOF","MATH","MAFIA","JOKER","CARROT,TOP","ROWDY"};

				case Themes.SOCIETY:
					
					return new string[] {"KIDS","PEOPLE","THINGS,HAVE,CHANGED","ALL MY,LIFE","STREETS","WORKING,HARD","INEQUALITY","CRAZY","LOVE","WHAT IS, NEXT","CULTURE","FIGHTING","FREEDOM","BEAUTY","JUNGLE","ROUGH","POWER","WORLD","BROOKLYN,BRIDGE","CHALLENGES","FIEND","MONEY","SPIRITS","DESERT","SKY","FLY", "WHY"};

				default:
					
					break;
			}	
				
				
					
					
		//string[,,] verses = new string[2,2,10] {{{"MY NAME IS JACK AND I'M HERE TO SAY", "THAT I BE BOASTING LIKE EVERY DAY", "I DO MY SHIT AND I DO IT FOR A WHILE", "AND I DO IT WITH STYLE"}, {"MY NAME IS GOOF AND I'M HERE TO SAY", "THAT I BE BOASTING LIKE EVERY DAY", "I DO MY SHIT AND I DO IT FOR A WHILE", "AND I DO IT WITH STYLE"}, {"MY NAME IS GOOP AND I'M HERE TO SAY", "THAT I BE BOASTING LIKE EVERY DAY", "I DO MY SHIT AND I DO IT FOR A WHILE", "AND I DO IT WITH STYLE"}}, {{"SNAGGLE TROLL0", "DINGLE DANGLE", "HAKUNA MATATA", "YADDA YADDA"},{"SNAGGLE1 TROLL", "DINGLE DANGLE", "HAKUNA MATATA", "YADDA YADDA"},{"SNAGGLE TROLL2", "DINGLE DANGLE", "HAKUNA MATATA", "YADDA YADDA"},{"MY NAME IS,__AND IM,HERE TO,SAY", "I LIKE TO,__AND I,LIKE TO,PLAY", "MY RHYMES,ARE__,AND MY,RHYMES ARE,SICK", "AND IM THE,__ OF MY,CLIQUE", "I CAME,FROM __,NOW IM AT,THE TOP", "I ONLY __,I NEVER, FLOP", "IM SO,WEALTHY I,______,FOR DINNER", "AND MY,LOVER YEAH,HES A __","IM A,GOLDEN,CHILD","IM TOO __,TO HANDLE", "IM __ THAN,A SHARK","IM __,THAN A,CANDLE"}}}; 
		//string[,,] verses2 = new string[,,] {{{"V1", "V2", "V3", "V4"}, {"V5", "V6", "V7", "V8"}, {"V9", "V10", "V11", "V12"}}, {{"V14", "V15", "V16", "V17"},{"V18", "V19", "V20", "V21"},{"V22", "V23", "V24", "V25"}}}; 
			
    	}
		return new string[] {"DEFAULT"};
	}
	

		
		
	private void OnNeighborRemove(Cube cube1, Cube.Side side1, Cube cube2, Cube.Side side2)  {
      Log.Debug("Neighbor remove: {0}.{1} <-> {2}.{3}", cube1.UniqueId, side1, cube2.UniqueId, side2);

      CubeWrapper wrapper1 = (CubeWrapper)cube1.userData;
	  CubeWrapper wrapper2 = (CubeWrapper)cube2.userData;	
      if (wrapper1 != null && wrapper2 != null) {
		if (currentPhase == PhaseType.BEATSELECT2) {		
	      if ((wrapper1.mType == CubeType.SELECTOR && wrapper2.mType == CubeType.SELECTABLE) || (wrapper2.mType == CubeType.SELECTOR && wrapper1.mType == CubeType.SELECTABLE)) {			
	        currentPhase = PhaseType.BEATSELECT;
			readyToChangeState = true;

		  }
		}
	  }
    }	
			
		
	private PlayerNames nameSelector (Cube.Side selectedSide, CubeWrapper wrapper) {
		if (wrapper.mCubeName == CubeName.NAMES1) {
			switch (selectedSide) {
			  case (Cube.Side.TOP):
				return PlayerNames.JO;
			  case (Cube.Side.BOTTOM):
				return PlayerNames.BO;
			  case (Cube.Side.LEFT):
				return PlayerNames.BRO;
			  case (Cube.Side.RIGHT):
				return PlayerNames.FLO;
			  default:
				return PlayerNames.JO;
			}
			
		}
		
		else if (wrapper.mCubeName == CubeName.NAMES2) {
			switch (selectedSide) {
			  case (Cube.Side.TOP):
				return PlayerNames.R2D2;
			  case (Cube.Side.BOTTOM):
				return PlayerNames.RADTAD;
			  case (Cube.Side.LEFT):
				return PlayerNames.STU;
			  case (Cube.Side.RIGHT):
				return PlayerNames.CHUMP;
			  default:
				return PlayerNames.JO;
			}
		}
			
		else return PlayerNames.JO;
	}
		
	private string beatSelector(Cube.Side selectedSide, CubeWrapper wrapper) {
		if (wrapper.mCubeName == CubeName.BEAT1) {
			switch (selectedSide) {
			  case (Cube.Side.TOP):
				return "basicbeatz";
			  case (Cube.Side.BOTTOM):
				return "reggae";
			  case (Cube.Side.LEFT):
				return "basicbeatz2";
			  case (Cube.Side.RIGHT):
				return "basicbeatz3";
			  default:
				return "basicBeat";
			} 
		}
		
		else if (wrapper.mCubeName == CubeName.BEAT2) {
			switch (selectedSide) {
			  case (Cube.Side.TOP):
				return "indian";
			  case (Cube.Side.BOTTOM):
				return "Riff2";
			  case (Cube.Side.LEFT):
				return "blues";
			  case (Cube.Side.RIGHT):
				return "rock1";
			  default:
				return "basicBeat";
			}
		}
		
		else return "";
	}
		
	private Themes themeSelector(Cube.Side selectedSide, CubeWrapper wrapper) {
		if (wrapper.mCubeName == CubeName.THEMES1) {
			switch (selectedSide) {
			  case (Cube.Side.TOP):
				return Themes.BOAST;

			  case (Cube.Side.BOTTOM):
				return Themes.ROAST;
				
			  case (Cube.Side.LEFT):
				return Themes.LOVE;
				
			  case (Cube.Side.RIGHT):
				return Themes.SOCIETY;
				
			  default:
				return Themes.BOAST;
			} 
		}
		
		else if (wrapper.mCubeName == CubeName.THEMES2) {
			switch (selectedSide) {
			  case (Cube.Side.TOP):
				return Themes.ANIMALS;
				
			  case (Cube.Side.BOTTOM):
				return Themes.GEOGRAPHY;
				
			  case (Cube.Side.LEFT):
				return Themes.POLITICS;
				
			  case (Cube.Side.RIGHT):
				return Themes.CHILDHOOD;
				
			  default:
				return Themes.ANIMALS;
			}
		}
		
		else return Themes.ANIMALS;		
	}
		
    override public void Tick()
    {
	  ticks++;
	  if (ticks > 9999 && ticksWaiting < ticks) {
	  ticks = 0;			
	  }
	  
	  // if phase has been switched, perform behavior for updating to new phase	
	
	  if (readyToChangeState) {
	    switch (currentPhase) {
		  		case (PhaseType.MODESELECT):
					break;
				case (PhaseType.NAMESELECT):
					currentVerseIndex = 0;
					readyToChangeState = false;
					
					fxMusic = Sounds.CreateSound("name");
					if (mMusic.IsPlaying) {
						mMusic.SetVolume((float).5);
					}
					
					fxMusic.Play(1,1);
					
					
					mWrappers[0].mImage = "nameSelect";
					mWrappers[0].mType = CubeType.SELECTABLE;
					mWrappers[0].mCubeName = CubeName.NAMES1;
					
					mWrappers[1].mImage = "nameSelect2";
					mWrappers[1].mType = CubeType.SELECTABLE;
					mWrappers[1].mCubeName = CubeName.NAMES2;

					mWrappers[2].mImage = "selector";
					mWrappers[2].mType = CubeType.SELECTOR;
					mWrappers[2].mCubeName = CubeName.SELECTOR;
					
					foreach (CubeWrapper wrapper in mWrappers) {
					  wrapper.textOrImage = DisplayType.IMAGE;
        			  wrapper.DrawSlide();
					}
					break;

					
				case (PhaseType.BEATSELECT):
					if (sampleMusic != null && sampleMusic.IsPlaying) 
			           sampleMusic.Stop();
					mWrappers[0].mImage = "beatSelect";
					mWrappers[0].mType = CubeType.SELECTABLE;
					mWrappers[0].mCubeName = CubeName.BEAT1;
					mWrappers[1].mImage = "beatSelect2";
					mWrappers[1].mType = CubeType.SELECTABLE;
					mWrappers[1].mCubeName = CubeName.BEAT2;
					mWrappers[2].mImage = "selector";
					mWrappers[2].mType = CubeType.SELECTOR;
					mWrappers[2].mCubeName = CubeName.SELECTOR;
					readyToChangeState = false;
					foreach (CubeWrapper wrapper in mWrappers) {
					  wrapper.textOrImage = DisplayType.IMAGE;
        			  wrapper.DrawSlide();
					}
					break;

					
				case (PhaseType.BEATSELECT2):
					foreach (CubeWrapper wrapper in mWrappers) {
        			  if (wrapper.mCubeName == CubeName.SELECTOR) {
							wrapper.mImage = "continue";
							wrapper.DrawSlide();
					   }
					 }
					 readyToChangeState = false;
					 break;

					
				case (PhaseType.THEMESELECT):
					mWrappers[0].mImage = "themeSelect";
					mWrappers[0].mType = CubeType.SELECTABLE;
					mWrappers[0].mCubeName = CubeName.THEMES1;
					mWrappers[1].mImage = "themeSelect2";
					mWrappers[1].mType = CubeType.SELECTABLE;
					mWrappers[1].mCubeName = CubeName.THEMES2;
					mWrappers[2].mImage = "selector";
					mWrappers[2].mType = CubeType.SELECTOR;
					mWrappers[2].mCubeName = CubeName.SELECTOR;
					readyToChangeState = false;
					fxMusic = Sounds.CreateSound("theme");
					fxMusic.Play(1, 1);
					
					
					foreach (CubeWrapper wrapper in mWrappers) {
        			  wrapper.DrawSlide();
					}
					break;
					// do sound fx
					
				case (PhaseType.RAPSTART1):
					mWrappers[0].mImage = "dis";
					mWrappers[0].mType = CubeType.NONE;
					mWrappers[0].mCubeName = CubeName.DIS;
					mWrappers[1].mImage = "dis";
					mWrappers[1].mType = CubeType.NONE;
					mWrappers[1].mCubeName = CubeName.DIS;
					mWrappers[2].mImage = "continue";
					mWrappers[2].mType = CubeType.NONE;
					mWrappers[2].mCubeName = CubeName.MIC;
					readyToChangeState = false;
					fxMusic = Sounds.CreateSound("micdis");
					fxMusic.Play(1, 1);
					
					foreach (CubeWrapper wrapper in mWrappers) {
        			  wrapper.DrawSlide();
					}
					
					verseLines = initializeVerseLines();		
					
					break;
					// do sound fx
					
				case (PhaseType.RAPSTART2):
					mWrappers[2].mImage = "3";
					mWrappers[2].DrawSlide();
					ticksWaiting = ticks + 20;
					readyToChangeState = true;
					currentPhase = PhaseType.RAPSTART3;
					break;
					
					// do sound fx
					
				case (PhaseType.RAPSTART3):
					if (ticks == ticksWaiting) {
						mWrappers[2].mImage = "2";
						mWrappers[2].DrawSlide();
						ticksWaiting = ticks + 20;
						readyToChangeState = true;
						currentPhase = PhaseType.RAPSTART4;
					}
					break;	
					// do sound fx
				
				case (PhaseType.RAPSTART4):
					if (ticks == ticksWaiting) {
						mWrappers[2].mImage = "1";
						mWrappers[2].DrawSlide();
						ticksWaiting = ticks + 20;
						readyToChangeState = true;
						currentPhase = PhaseType.RAPSTART5;
					}
					break;	
					// do sound fx
					
				case (PhaseType.RAPSTART5):
					if (ticks == ticksWaiting) {
						mWrappers[2].mImage = "go";
						mWrappers[2].DrawSlide();
						ticksWaiting = ticks + 20;
						readyToChangeState = true;
						currentPhase = PhaseType.LYRICS;
					}
					break;		
					
				case (PhaseType.LYRICS):
					if (ticks == ticksWaiting) {
      					mMusic.Play(1, -1);
						readyToChangeState = false;
						
						foreach (CubeWrapper wrapper in mWrappers) {
							wrapper.textOrImage = DisplayType.TEXT;
							string newText = verseLines[currentVerseIndex];
							wrapper.mText.setText(newText);
							wrapper.DrawSlide();	
						}
					}
					break;
					
				case (PhaseType.DOUBLEDIS):
					foreach (CubeWrapper wrapper in mWrappers) {
						if ((wrapper.mCubeName == CubeName.JUSTDISSED) || (wrapper.mCubeName == CubeName.MIC)) {
							wrapper.mImage = "dis";
						    wrapper.textOrImage = DisplayType.IMAGE;
						}
						else if (wrapper.mCubeName == CubeName.DIS) {
							wrapper.mImage = "doubledis";
							wrapper.textOrImage = DisplayType.IMAGE;
						}
						wrapper.DrawSlide();
					}
					readyToChangeState = false;
					break;
					
				case (PhaseType.ENDOFRAP):
						mWrappers[0].mImage = "max";
						mWrappers[1].mImage = "nice";
					    mWrappers[2].mImage = "continue";
					    
					    foreach (CubeWrapper wrapper in mWrappers) {
							wrapper.textOrImage = DisplayType.IMAGE;
							wrapper.DrawSlide();                      
						}
						readyToChangeState = false;
					    break;
					
				case (PhaseType.TRIVIA1):
					
					fxMusic = Sounds.CreateSound("trivia");
					fxMusic.Play(1, 1);
					
					foreach (CubeWrapper wrapper in mWrappers) {
					  if (wrapper.mCubeName == CubeName.MIC) {
							wrapper.mCubeName = CubeName.DEFENDER;
							wrapper.textOrImage = DisplayType.IMAGE;
							wrapper.mImage = "buzzwer";
					  }
					  else if (wrapper.mCubeName == CubeName.JUSTDISSED) {
							wrapper.mCubeName = CubeName.CHALLENGER;
							wrapper.textOrImage = DisplayType.IMAGE;
							wrapper.mImage = "buzzwer";
					  }
					  else if (wrapper.mCubeName == CubeName.DIS) {
							wrapper.mCubeName = CubeName.QUESTION;
							wrapper.textOrImage = DisplayType.TEXT;
							wrapper.mText.setText(triviaQuestions[triviaIndex]);
					  }
					  wrapper.DrawSlide();	
					  readyToChangeState = false;
					}
					break;
					
				case (PhaseType.TRIVIA2):
					foreach (CubeWrapper wrapper in mWrappers) {
						if (wrapper.mCubeName == buzzer) {
							wrapper.mImage = "buzzerWin";
						}
						else if (wrapper.mCubeName == CubeName.QUESTION) {
							wrapper.mCubeName = CubeName.ANSWER;
							wrapper.mText.setText(triviaAnswers[triviaIndex]);
						}
						else { // must be the one that didn't buzz in
							wrapper.mImage = "buzzerLose";
						}
						
						readyToChangeState = false;
						wrapper.DrawSlide();
						
					}
					triviaIndex++;
					break;
					
				case (PhaseType.TRIVIA3):
					foreach (CubeWrapper wrapper in mWrappers) {
						if (wrapper.mCubeName == CubeName.ANSWER) {
							wrapper.textOrImage = DisplayType.IMAGE;
							wrapper.mImage = "pressorshake";
							wrapper.DrawSlide();
							readyToChangeState = false;
						}
					}
					break;
					
				case (PhaseType.SCORES):
					mWrappers[0].textOrImage = DisplayType.TEXT;
					mWrappers[1].textOrImage = DisplayType.TEXT;
					mWrappers[2].textOrImage = DisplayType.IMAGE;
					mWrappers[2].mImage = "continue";
					mWrappers[2].DrawSlide();
					string scoreText = makeScoreString();
				    mWrappers[1].mText.setText(scoreText);
					mWrappers[0].mText.setText("LOOK AT,YOUR SCORE");
					mWrappers[0].DrawSlide();
					mWrappers[1].DrawSlide();
					break;		
					
				default:
					break;
				}
	  }
		
	  else if (ticks == ticksWaiting) {
			switch (currentPhase) {
				case (PhaseType.DOUBLEDIS):
						currentPhase = PhaseType.TRIVIA1;
						readyToChangeState = true;
						break;
				// add in the RapStart part	
				default:
					break;
			}
      }
	}
		
	public string makeScoreString() {
		string result = "SCORE     ";
			string name = "";
			PlayerNames thisPlayer = currentPlayer;
			switch (thisPlayer) {
					case (PlayerNames.JO):
						name = "JO";
						break;
					case (PlayerNames.BO):
						name = "BO";
						break;
					case (PlayerNames.BRO):
						name = "BRO";
						break;
					case (PlayerNames.CHUMP):
						name = "CHUMP";
						break;
					case (PlayerNames.FLO):
						name = "FLO";
						break;
					case (PlayerNames.R2D2):
						name = "R2D2";
						break;
					case (PlayerNames.RADTAD):
						name = "RAD TAD";
						break;
					case (PlayerNames.STU):
						name = "STU";
						break;
					default:
						 name = "";
						 break;
	         }
			 result = result + name + "  " + scores[(int) currentPlayer];
			return result;
		   }		
				
    // development mode only
    // start FreeStyle as an executable and run it, waiting for Siftrunner to connect
    static void Main(string[] args) { new FreeStyleApp().Run(); }
  

  }

}