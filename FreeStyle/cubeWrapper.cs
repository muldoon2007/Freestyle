using Sifteo;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FreeStyle
{
   public class CubeWrapper {

    public FreeStyleApp mApp;
    public Cube mCube;
    public CubeName mCubeName;
	public string mImage = "";
	public DisplayType textOrImage = DisplayType.IMAGE;
	public CubeType mType;
	public myText mText = new myText();

    // This flag tells the wrapper to redraw the current image on the cube. (See Tick, below).
    public bool mNeedDraw = false;

    public CubeWrapper(FreeStyleApp app, Cube cube) {
      mApp = app;
      mCube = cube;
      mCube.userData = this;

      // Here we attach more event handlers for button and accelerometer actions.
      mCube.ButtonEvent += OnButton;
      mCube.ShakeStoppedEvent += OnShakeStopped;
      mCube.FlipEvent += OnFlip;
    }

    // ## Button ##
    // This is a handler for the Button event. It is triggered when a cube's
    // face button is either pressed or released. The `pressed` argument
    // is true when you press down and false when you release.
	private void OnButton(Cube cube, bool pressed) {
      if (pressed) {
        Log.Debug("Button pressed");
      } 
	  else {
        Log.Debug("Button released");

        switch (mApp.currentPhase) {
				case (PhaseType.BEATSELECT2):
					mApp.currentPhase = PhaseType.RAPSTART1;
					mApp.readyToChangeState = true;
					if (mApp.sampleMusic.IsPlaying)
						mApp.sampleMusic.Stop();
					mApp.mMusic = mApp.sampleMusic;
					break;
				case (PhaseType.RAPSTART1):
					mApp.currentPhase = PhaseType.RAPSTART2;
					mApp.readyToChangeState = true;
					break;
				case (PhaseType.LYRICS):
					if (mCubeName == CubeName.MIC) {
			
						mApp.currentVerseIndex++;
						mApp.scores[(int) mApp.currentPlayer] = mApp.scores[(int) mApp.currentPlayer] + 1;
						if (mApp.currentVerseIndex < mApp.verseLines.Length) {
						
						   foreach (CubeWrapper wrapper in mApp.mWrappers) {
							  wrapper.mText.setText(mApp.verseLines[mApp.currentVerseIndex]);
							  wrapper.DrawSlide();  
						   }
						}
						else {
							mApp.currentPhase = PhaseType.ENDOFRAP;
							mApp.readyToChangeState = true;
							if (mApp.mMusic.IsPlaying) mApp.mMusic.Stop();
						}
					}
					
					else if (mCubeName == CubeName.DIS) {
						
						mApp.fxMusic = mApp.Sounds.CreateSound("dis");
						mApp.fxMusic.Play(1, 1);
						mCubeName = CubeName.JUSTDISSED;
						mApp.currentPhase = PhaseType.DOUBLEDIS;
						mApp.readyToChangeState = true;
						mApp.ticksWaiting = mApp.ticks + 60;
						mApp.mMusic.Stop();
					}		
					break;
					
				case (PhaseType.DOUBLEDIS):
					if (mCubeName == CubeName.DIS) {
						mApp.currentPhase = PhaseType.SCORES;
						mApp.fxMusic = mApp.Sounds.CreateSound("doubledis");
						mApp.fxMusic.Play(1, 1);
						mApp.readyToChangeState = true;
					}
					break;
					
				case (PhaseType.TRIVIA1):
					if (mCubeName == CubeName.CHALLENGER || mCubeName == CubeName.DEFENDER) {
					    mApp.buzzer = mCubeName;
						mApp.currentPhase = PhaseType.TRIVIA2;
						mApp.readyToChangeState = true;
						mApp.fxMusic = mApp.Sounds.CreateSound("ding");
						mApp.fxMusic.Play(1,1);
					}
					break;
					
				case (PhaseType.TRIVIA2):
					if (mCubeName == CubeName.ANSWER) {
						mApp.currentPhase = PhaseType.TRIVIA3;
						mApp.readyToChangeState = true;
			
					}
					break;
					
				case (PhaseType.TRIVIA3):
					if (mCubeName == CubeName.ANSWER) {
					  if (mApp.buzzer == CubeName.DEFENDER) {	
					
						mApp.scores[(int)mApp.currentPlayer] = mApp.scores[(int)mApp.currentPlayer] + 1;
						mApp.currentPhase = PhaseType.RAPSTART1;
						mApp.readyToChangeState = true;
					  }
						
					  else if (mApp.buzzer == CubeName.CHALLENGER) {
							
					    mApp.currentPhase = PhaseType.SCORES;
						mApp.readyToChangeState = true;
					  }
					}
					break;
					
				case (PhaseType.SCORES):
					if (mImage == "continue") {
						mApp.currentPhase = PhaseType.NAMESELECT;
						mApp.readyToChangeState = true;
					}
					break;
					
				case (PhaseType.ENDOFRAP):
					if (mImage == "continue") {
						mApp.currentPhase = PhaseType.SCORES;
						mApp.readyToChangeState = true;
					}
					break;
					
				default:
					break;
		}
      }
    }
		
    private void OnShakeStarted(Cube cube) {
      Log.Debug("Shake start");
			
    }


    private void OnShakeStopped(Cube cube, int duration) {
      Log.Debug("Shake stop: {0}", duration);
      		switch (mApp.currentPhase) {
			case (PhaseType.TRIVIA3):
				if (mCubeName == CubeName.ANSWER) {
					if (mApp.buzzer == CubeName.DEFENDER) {
						mApp.currentPhase = PhaseType.SCORES;
						mApp.readyToChangeState = true;
						mApp.currentVerseIndex = 0;
					}
					else if (mApp.buzzer == CubeName.CHALLENGER) {
						mApp.currentPhase = PhaseType.RAPSTART1;
						mApp.readyToChangeState = true;
					}
						
				}
				break;
			default:
				break;
		}
			
    }

    private void OnFlip(Cube cube, bool newOrientationIsUp) {
      if (newOrientationIsUp) {
        Log.Debug("Flip face up");
        mNeedDraw = true;
      } else {
        Log.Debug("Flip face down");
        mNeedDraw = true;
      }
    }

    // ## Cube.Image ##
    // This method draws the current image to the cube's display. The
    // Cube.Image method has a lot of arguments, but many of them are optional
    // and have reasonable default values.
    public void DrawSlide() {

      
      mCube.FillScreen(Color.White);
      if (textOrImage == DisplayType.IMAGE) {
        mCube.Image(mImage);
	  }
			
	  else if (textOrImage == DisplayType.TEXT) {
		mText.writeText(mCube);
	  }
	  mCube.Paint();		
    }

		
  public class myText
    { 
     private int mXstart = 10;
     private int mYstart = 26;			
	 private int mTextH = 16;
	 private int mTextW = 8;
	 private bool mWrap = true;   //wrap allows the text to wrap across the screen if extend is 
		                         //enabled wrap will only wrap after reaching the end of the cubes
     private string mString = "0123456789";
	 private Color mColor;	
	 //writes/draws text to cube
			
	 public void writeText(Cube cube){
			int i;
			int rowCount = 1;
			int nextX = mXstart;
		    int nextY = mYstart;
			
			for( i = 0; i < mString.Length; i++){
				
				if((mWrap == true && nextX > 118) || mString[i] == ','){
				   rowCount++;
				   nextY = mYstart + (rowCount - 1) *(mTextH + 6);
				   nextX = mXstart;	
				}
				
			    printChar(cube,mString[i],nextX,nextY);
				if (mString[i] != ',')
					nextX = nextX + (mTextW + 3);

				}
		     }
	
	 public void setStringOrig(int x, int y){
		    mXstart = x;
		    mYstart = y;
				
		}
			
	 public void setText(string lString){
			mString = string.Copy (lString);
		}
			
			
	//draws all chars and numbers A-Z 0-9	
	  public void printChar(Cube cube, char ch, int x,int y, Color c){
			 mColor = c;
			 printChar( cube, ch, x,y);
			}
			
	 public void printChar(Cube cube, char c, int x,int y){
	  switch( c) {
			case 'A':   //Draw A
			  cube.FillRect(mColor,x, y + 2 - mTextH, 2, mTextH - 2); 
			  cube.FillRect(mColor,x+2,y - mTextH ,mTextW - 2,2);
              cube.FillRect(mColor,x+2,y - mTextH/2,mTextW,2);  
              cube.FillRect(mColor,x+mTextW,y + 2 - mTextH,2,mTextH - 2); 
		      break;
			case 'B' :  //Draw B
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
			  cube.FillRect(mColor, x, y - mTextH/2, mTextW, 2);
              cube.FillRect(mColor, x + mTextW, y + 2 -mTextH, 2, mTextH/2-2);
			  cube.FillRect(mColor, x + mTextW, y +2 -mTextH/2, 2, mTextH/2-4);	
              cube.FillRect(mColor, x +2, y - 2 , mTextW - 2, 2);
              cube.FillRect(mColor, x +2, y - mTextH, mTextW - 2, 2);	
			  break;
			case 'C' : //Draw C
			  cube.FillRect(mColor, x, y + 2 - mTextH, 2, mTextH-4);
              cube.FillRect(mColor, x +2, y - 2 , mTextW, 2);
              cube.FillRect(mColor, x +2, y - mTextH, mTextW, 2);
			  break;
			case 'D' : //Draw D
              cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + mTextW, y + 2 -mTextH, 2, mTextH-4);
              cube.FillRect(mColor, x +2, y - 2 , mTextW - 2, 2);
              cube.FillRect(mColor, x +2, y - mTextH, mTextW - 2, 2);
			  break;
			case 'E' : //Draw E	
              cube.FillRect(mColor, x, y- mTextH, 2, mTextH);
              cube.FillRect(mColor, x+2, y - mTextH, mTextW, 2);
              cube.FillRect(mColor, x+2, y - 2- mTextH/2, mTextW/2, 2);
              cube.FillRect(mColor, x+2, y - 2, mTextW, 2);	
			  break;	
			case 'F' : //Draw F	
              cube.FillRect(mColor, x, y- mTextH, 2, mTextH);
              cube.FillRect(mColor, x+2, y - mTextH, mTextW, 2);
              cube.FillRect(mColor, x+2, y - 2 - mTextH/2, mTextW/2, 2);
			  break;
			case 'G' : //Draw G
			  cube.FillRect(mColor, x, y + 2- mTextH, 2, mTextH - 4);
              cube.FillRect(mColor, x + mTextW, y - 2 -mTextH/3, 2, mTextH/3);
			  cube.FillRect(mColor, x + mTextW, y - 4- 3*mTextH/4, 2, mTextH/4);
			  cube.FillRect(mColor, x+ mTextW/2, y - mTextH/2, mTextW/2 + 2, 2);
              cube.FillRect(mColor, x +2, y - 2 , mTextW - 2, 2);
              cube.FillRect(mColor, x +2, y - mTextH, mTextW - 2, 2);	
			  break;	
			case 'H' : //Draw H
              cube.FillRect(mColor, x, y - mTextH, 2, mTextH  );   
              cube.FillRect(mColor, x + 2, y - mTextH/2, mTextW,2);  
              cube.FillRect(mColor, x+mTextW, y - mTextH, 2, mTextH ); 
		      break;
			case 'I' :  //Draw I
			  cube.FillRect(mColor, x + mTextW/2, y - mTextH, 2, mTextH  );
			  cube.FillRect(mColor, x + 2,y - mTextH, mTextW-2,2);  
			  cube.FillRect(mColor, x + 2,y - 2, mTextW-2,2);	
			  break;
			case 'J' : //Draw J
			  cube.FillRect(mColor, x + mTextW, y - mTextH, 2, mTextH - 2 );
			  cube.FillRect(mColor, x + mTextW/2, y - mTextH, mTextW/2,2); 
			  cube.FillRect(mColor, x + 2, y - 2, mTextW - 2, 2);
			  cube.FillRect(mColor, x, y - 2  - mTextH/4, 2, mTextH/4 );
			  break;
			case 'K' : //Draw K
			  cube.FillRect(mColor,x, y - mTextH, 2, mTextH  );
			  cube.FillRect(mColor,x + 2, y - 2 - mTextH/2,2, 2);
			  cube.FillRect(mColor, x + 4, y - mTextH/2, 2,mTextH/4);
			  cube.FillRect(mColor, x + 4, y - 3*mTextH/4, 2, mTextH/4 - 2);
			  cube.FillRect(mColor, x + 6, y - mTextH, 2, mTextH/4);		
			  cube.FillRect(mColor, x + 6, y - mTextH/4, 2, mTextH/4);	
			  break;
			case 'L':  //Draw L
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + 2, y - 2, mTextW - 2, 2);
				
			  break;
				
			case 'M' :  //Draw M
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + 2, y - mTextH, mTextW, 2);
              cube.FillRect(mColor, x + mTextW/2, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + mTextW, y- mTextH, 2, mTextH);	
			  break;
				
			case 'N' :  //Draw N
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
			  cube.FillRect(mColor, x + 2, y + 2 - mTextH, 2,2);
			  cube.FillRect(mColor, x + 4, y + 4 - mTextH, (mTextW-5)/2,(mTextH -5)/2);  
			  cube.FillRect(mColor, x - 4 + mTextW, y -4 - (mTextH - 5)/2 , (mTextW-5)/2,(mTextH - 5)/2);
			  cube.FillRect(mColor, x - 2 + mTextW, y -4, 2,2);
			  cube.FillRect(mColor, x+ mTextW, y - mTextH, 2, mTextH);	
			  break;
			
			case 'O' :	//Draw O
              cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + mTextW, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x+2, y - 2, mTextW, 2);
              cube.FillRect(mColor, x+2, y - mTextH, mTextW, 2);
			  break;
			
			case 'P'  : //Draw P
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + mTextW, y + 2 - mTextH, 2, mTextH/2 - 2);
              cube.FillRect(mColor, x + 2, y - mTextH,   mTextW - 2, 2);
              cube.FillRect(mColor, x + 2, y - mTextH/2, mTextW - 2, 2);
              //cube.FillRect(mColor, x + mTextW, y - mTextH/2, 2, mTextH/2);
			  break;
			case 'Q'  : //Draw Q
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + mTextW, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x+2, y - 2, mTextW, 2);
              cube.FillRect(mColor, x+2, y - mTextH, mTextW, 2);
			  cube.FillRect(mColor, x - 4 + mTextW, y - mTextH/2, 2,mTextH/4);
			  cube.FillRect(mColor, x -2 +  mTextW, y - mTextH/4, 2, mTextH/4);		
			  break;
			case 'R'  : //Draw R
              cube.FillRect(mColor, x, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x + mTextW, y + 2 - mTextH, 2, mTextH/2 - 2);
              cube.FillRect(mColor, x + 2, y - mTextH,   mTextW - 2, 2);
              cube.FillRect(mColor, x + 2, y - mTextH/2, mTextW - 2, 2);
			  cube.FillRect(mColor, x - 2 + mTextW, y - mTextH/2, 2,mTextH/4);
			  cube.FillRect(mColor, x     + mTextW, y - mTextH/4, 2, mTextH/4);	
			  break;
			case 'S'  : //Draw S
			  cube.FillRect(mColor, x + 2, y - mTextH, mTextW - 2, 2);	
			  cube.FillRect(mColor, x, y + 2 - mTextH , 2, mTextH/2 - 4);
			  cube.FillRect(mColor, x + 2, y - mTextH/2 - 2, mTextW - 4, 2);
			  cube.FillRect(mColor, x + mTextW - 2, y - mTextH/2, 2, mTextH/2 - 2);	
			  cube.FillRect(mColor, x , y - 2, mTextW - 2, 2);	
			  break;
			case 'T'  : //Draw T
			  cube.FillRect(mColor, x + mTextW/2 - 1, y - mTextH, 2, mTextH  );
			  cube.FillRect(mColor, x ,y - mTextH, mTextW,2);  
			  break;
			case 'U'  : //Draw U
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH - 2);
              cube.FillRect(mColor, x + mTextW, y- mTextH, 2, mTextH - 2);
              cube.FillRect(mColor, x+2, y - 2, mTextW - 2, 2);
			  break;
			case 'V'  : //Draw V
			 cube.FillRect(mColor, x - 4 + mTextW/2, y - mTextH, 2, mTextH - 4);	 
			  cube.FillRect(mColor, x - 2 + mTextW/2,y - 4, 2, 2); 
			  cube.FillRect(mColor, x + mTextW/2, y - 2, 2, 2);
			  cube.FillRect(mColor, x + 2 + mTextW/2,y - 4, 2, 2);
			  cube.FillRect(mColor, x + 4 + mTextW/2, y - mTextH, 2, mTextH - 4);	
			  break;
			case 'W'  :	//Draw W			
              cube.FillRect(mColor, x, y- mTextH, 2, mTextH);
              cube.FillRect(mColor, x + 2, y - 2 , mTextW, 2);
              cube.FillRect(mColor, x + mTextW/2, y- mTextH/2, 2, mTextH/2 );
              cube.FillRect(mColor, x + mTextW, y - mTextH, 2, mTextH);
		      break;
							
			case 'X'  : //Draw X
			  cube.FillRect(mColor, x - 4 + mTextW/2, y - mTextH, 2, mTextH/2 - 2);	 
			  cube.FillRect(mColor, x - 2 + mTextW/2, y - 2 - mTextH/2, 2, 2); 
			  cube.FillRect(mColor, x + mTextW/2, y - mTextH/2, 2, 2);
			  cube.FillRect(mColor, x + 2 + mTextW/2, y - 2 - mTextH/2, 2, 2);
			  cube.FillRect(mColor, x + 4 + mTextW/2, y - mTextH, 2, mTextH/2 - 2);
			
			  cube.FillRect(mColor, x - 4 + mTextW/2, y + 4- mTextH/2, 2, mTextH/2 - 4);	 
			  cube.FillRect(mColor, x - 2 + mTextW/2, y + 2 - mTextH/2, 2, 2); 

			  cube.FillRect(mColor, x + 2 + mTextW/2, y + 2 - mTextH/2, 2, 2);
			  cube.FillRect(mColor, x + 4 + mTextW/2, y + 4- mTextH/2, 2, mTextH/2 - 4);	
			  	
				
			  break;
			case 'Y'  : //Draw Y
			  cube.FillRect(mColor, x - 4 + mTextW/2, y - mTextH, 2, mTextH/2 - 2);	 
			  cube.FillRect(mColor, x - 2 + mTextW/2,y - 2 - mTextH/2, 2, 2); 
			  cube.FillRect(mColor, x + mTextW/2, y - mTextH/2, 2, mTextH/2);
			  cube.FillRect(mColor, x + 2 + mTextW/2,y - 2 - mTextH/2, 2, 2);
			  cube.FillRect(mColor, x + 4 + mTextW/2, y - mTextH, 2, mTextH/2 - 2);
			  break;
			case 'Z'  : //Draw Z
			  cube.FillRect(mColor, x, y - mTextH, mTextW, 2);
			  cube.FillRect(mColor, x - 2 + mTextW, y + 2 - mTextH, 2,2);
			  cube.FillRect(mColor, x - 4 + mTextW, y + 4 - mTextH, (mTextW-5)/2,(mTextH -5)/2 -2);
			 // cube.FillRect(mColor, x - 6 + mTextW, y + 4 - mTextH, 2, 2);	
		      cube.FillRect(mColor, x + 4, y - 4 - (mTextH - 5)/2  , (mTextW-5)/2,(mTextH - 5)/2 - 4); 
			  cube.FillRect(mColor, x + 2, y - 2 - (mTextH - 5)/2 ,  (mTextW-5)/2,(mTextH - 5)/2 - 2);	
			  cube.FillRect(mColor, x, y -4, 2,2);
			  cube.FillRect(mColor, x, y -2, mTextW, 2);	
			  break;
			case '0'  : //Draw 0
			  cube.FillRect(mColor, x, y + 2- mTextH, 2, mTextH - 4);
              cube.FillRect(mColor, x + mTextW, y + 2 - mTextH, 2, mTextH - 4);
              cube.FillRect(mColor, x+2, y - 2, mTextW - 2, 2);
              cube.FillRect(mColor, x+2, y - mTextH, mTextW - 2, 2);
			  break;
			case '1'  : //Draw 1
			  cube.FillRect(mColor, x + mTextW/2, y - mTextH, 2, mTextH  );
			  cube.FillRect(mColor, x - 2 +mTextW/2 ,y - mTextH, 2,2);  
			  cube.FillRect(mColor, x + 2,y - 2, mTextW-2,2);	
			  break;		
			case '2'  : //Draw 2
			  cube.FillRect(mColor, x , y - mTextH, mTextW - 2, 2);	
			  cube.FillRect(mColor, x + mTextW - 2, y + 2 - mTextH , 2, mTextH/2 - 4);
			  cube.FillRect(mColor, x + 2, y - mTextH/2 - 2, mTextW - 4, 2);
			  cube.FillRect(mColor, x , y - mTextH/2, 2, mTextH/2 - 2);	
			  cube.FillRect(mColor, x + 2, y - 2, mTextW - 4, 2);		
			  break;
			case '3'  : //Draw 3
			  cube.FillRect(mColor, x - 2 + mTextW, y + 2 - mTextH, 2, mTextH - 4);
              cube.FillRect(mColor, x, y - mTextH, mTextW - 2, 2);
              cube.FillRect(mColor, x - 2+ mTextW/2, y - mTextH/2, mTextW/2, 2);
              cube.FillRect(mColor, x, y - 2, mTextW - 2, 2);		
			  break;
			case '4'  : //Draw 4
			  cube.FillRect(mColor, x, y - mTextH, 2, mTextH/2  );   
              cube.FillRect(mColor, x + 2, y - mTextH/2, mTextW,2);  
              cube.FillRect(mColor, x -2 + mTextW, y - mTextH, 2, mTextH );	
			  break;
			case '5'  : //Draw 5
			  cube.FillRect(mColor, x , y - mTextH, mTextW , 2);	
			  cube.FillRect(mColor, x, y + 2 - mTextH , 2, mTextH/2 - 4);
			  cube.FillRect(mColor, x + 2, y - mTextH/2 - 2, mTextW - 4, 2);
			  cube.FillRect(mColor, x + mTextW - 2, y - mTextH/2, 2, mTextH/2 - 2);	
			  cube.FillRect(mColor, x , y - 2, mTextW - 2, 2);		
			  break;
			case '6'  : //Draw 6
			  cube.FillRect(mColor, x + 2, y - mTextH, mTextW - 2, 2);	
			  cube.FillRect(mColor, x, y + 2 - mTextH , 2, mTextH - 4);
			  cube.FillRect(mColor, x + 2, y - mTextH/2 - 2, mTextW - 4, 2);
			  cube.FillRect(mColor, x + mTextW - 2, y - mTextH/2, 2, mTextH/2 - 2);	
			  cube.FillRect(mColor, x + 2, y - 2, mTextW - 4, 2);		
			  break;
			case '7'  : //Draw 7
			  cube.FillRect(mColor, x - 2 + mTextW, y - mTextH, 2, mTextH);
              cube.FillRect(mColor, x , y - mTextH, mTextW - 2, 2);	
			  break;
			case '8'  : //Draw 8
			  cube.FillRect(mColor, x, y + 2- mTextH, 2, mTextH - 4);
              cube.FillRect(mColor, x - 2 + mTextW, y + 2 - mTextH, 2, mTextH - 4);
			  cube.FillRect(mColor, x + 2, y - 1 - mTextH/2, mTextW - 4, 2);	
              cube.FillRect(mColor, x + 2, y - 2, mTextW - 4, 2);
              cube.FillRect(mColor, x + 2, y - mTextH, mTextW - 4, 2);	
			  break;
			case '9'  : //Draw 9
			  cube.FillRect(mColor, x - 2 + mTextW, y + 2 - mTextH, 2, mTextH - 2);
              cube.FillRect(mColor, x , y + 2 - mTextH, 2, mTextH/2 - 2);
              cube.FillRect(mColor, x + 2, y - mTextH,   mTextW - 4, 2);
              cube.FillRect(mColor, x + 2, y - mTextH/2, mTextW - 4, 2);	
			  break;
			case '_'  :
			  cube.FillRect(mColor, x, y, mTextW + 4, 2);		
			  break;
					
			case '?' :
			  cube.FillRect(mColor, x, y + 2- mTextH, mTextW - 2, 2);
              cube.FillRect(mColor, x - 2 + mTextW, y + 2 - mTextH, 2, mTextH/2 - 2);
			  cube.FillRect(mColor, x, y - 1 - mTextH/2, mTextW - 2, 2);	
              cube.FillRect(mColor, x, y + 1 - mTextH/2, 2, mTextH/2 - 3);
              cube.FillRect(mColor, x, y, 2, 2);		
			  break;	
			
			case '\'':
	 		  cube.FillRect(mColor, x - 2 + mTextW/2, y + 2 - mTextH, 2, mTextH/3);
			  break;
			
			case '.':
			   cube.FillRect(mColor, x, y, 2, 2);
			   break;
					
			}

			
		}
		
	//todo: add support for sprite/image based fonts
	//todo: add support for extending features
    } 
  }
}

