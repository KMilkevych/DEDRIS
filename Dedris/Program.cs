using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SFML.System;
using SFML.Graphics;
using SFML.Window;


namespace Dedris
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        
        static int type = 0;
        static int color = type;
        static Brick currentBrick; //The current tetrimino brick entity

        static int movex = 0; //Variable to hold value of current movement on x-axis

        //Game refreshing delays in ms
        static int updateDelay = 500;
        static int targetUpdateDelay = 500;

        //Game state variables
        static bool cheatsOn = false;
        static bool gamePaused = false;
        static bool playerDead = false;

        static int score = 0; //Score variable

        static string pressedKeys = ""; //Used for detecting cheatcodes

        static List<Vector2> deadblocks; //A list of coordinates to all "dead" (placed, or immovable) blocks
        static List<int> deadblocktypes; //A list of types (colors) to all "dead" (placed, or immovable) blocks

        static void Main()
        {            
            Random rnd = new Random();

            //int color = 0;
            //Image bgImg = DEDRIS.Properties.Resources.background;

            //Loading the textures required
            Texture txt = new Texture("Resources\\tiles.png");
            Texture bg = new Texture("Resources\\background.png"); //The offset is (28; 31)
            Texture frm = new Texture("Resources\\frame.png");

            Font scoreFont = new Font("Resources\\font.ttf"); //Setting the font for the score

            Sprite background = new Sprite(bg);
            Sprite frame = new Sprite(frm);

            Clock clock = new Clock();

            //Defining the actual deadblocks, and the colors of the dead blocks lists
            deadblocks = new List<Vector2>();
            deadblocktypes = new List<int>();

            //Setting the next tetrimino brick randomly
            type = rnd.Next(0, 7);
            color = type;
            currentBrick = new Brick(type, new Vector2(3, 0));

            //Creating the window
            RenderWindow window = new RenderWindow(new VideoMode(320, 480), "DEDRIS", Styles.Resize); //Define window
            window.SetFramerateLimit(60);
            window.Size = new Vector2u(640, 960);
            window.Position = new Vector2i(Convert.ToInt32((VideoMode.DesktopMode.Width - window.Size.X)/2), Convert.ToInt32((VideoMode.DesktopMode.Height - window.Size.Y) / 2));

            //Define events
            window.Closed += new EventHandler(OnClose);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPress);
            window.Resized += new EventHandler<SizeEventArgs>(OnResize);

            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(Color.White);

                List<Sprite> bricksprites = new List<Sprite> { };
                List<Sprite> blocksprites = new List<Sprite> { };

                //Apply movement to brick
                if (!gamePaused) //only apply if game is not paused
                {
                    currentBrick.baseCord.x += movex;
                }
               
                movex = 0; //nullify movement after it is applied

                //Move on the y axe
                if (clock.ElapsedTime.AsMilliseconds() >= updateDelay && !gamePaused)
                {
                    if (playerDead)
                    {
                        window.Close();
                    }

                    //Test if block for some stupid reason is out of vertical boundaries
                    foreach (Vector2 block in currentBrick.bricks)
                    {
                        if (block.x + currentBrick.baseCord.x < 0)
                        {
                            currentBrick.baseCord.x += 1; //Move the block/tetrimino back inside the game window
                        }

                        if (block.x + currentBrick.baseCord.x > 9) //9 is the width of the game window in tetrimino squares
                        {
                            currentBrick.baseCord.x -= 1; //Move the block/tetrimino back inside the game window
                        }
                    }

                    bool reachedBottom = false;
                    bool touchedBlock = false;

                    //Test if block has touched bottom
                    foreach (Vector2 block in currentBrick.bricks)
                    {
                        if (block.y + currentBrick.baseCord.y >= 19)
                        {
                            reachedBottom = true;
                        }
                    }

                    //Test if block has touched another block
                    if (!reachedBottom)
                    {
                        foreach (Vector2 block in currentBrick.bricks)
                        {
                            int blockY = Convert.ToInt32(block.y + currentBrick.baseCord.y); //Getting the exact coordinates for each block in the tetrimino; block.y is the local coordinate of the block inside
                            int blockX = Convert.ToInt32(block.x + currentBrick.baseCord.x); //of the tetrimino, and currentBrick.baseCord.y is the global coordinate of the start of the tetrimino.

                            foreach (Vector2 deadblock in deadblocks)
                            {
                                if (blockY + 1 == deadblock.y && blockX == deadblock.x)
                                {
                                    touchedBlock = true;
                                    break;
                                }
                            }
                            if (touchedBlock == true)
                            {
                                break;
                            }
                        }
                    }

                    //Add tetrimino's blocks to the deadblocks list and destroy the current tetrimino if it has touched bottom or touched another block; then calculates if a line has been created and so on
                    if (reachedBottom == true || touchedBlock == true)
                    {
                        if (currentBrick.baseCord.y < 2) //If block is stopped at the top of the screen, the player has died and the game should end
                        {
                            //window.Close();
                            playerDead = true;
                            gamePaused = true;
                        }

                        foreach (Vector2 block in currentBrick.bricks) //Looping through evert block in the tetrimino and adding it to the "dead" blocks
                        {
                            deadblocks.Add(new Vector2(block.x + currentBrick.baseCord.x, block.y + currentBrick.baseCord.y));
                            deadblocktypes.Add(currentBrick.type);
                        }
                        type = rnd.Next(0, 7); //Selecting the next tetrimino type randomly
                        color = type;
                        currentBrick = new Brick(type, new Vector2(3, 0)); //Creating the new tetrimino

                        //Test for existing line
                        int[] blocksInRow = new int[20] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}; //20 lines total, a list for holding number of blocks in each line/row
                        List<int> rowsToRemove = new List<int>(); //A list with the numbers of the rows that are going to be removed

                        foreach (Vector2 deadblock in deadblocks)
                        {
                            blocksInRow[Convert.ToInt32(deadblock.y)] += 1; //Adding 1 to the line number that this block lies in
                            if (blocksInRow[Convert.ToInt32(deadblock.y)] == 10) //If the row/line is filled up with blocks, it should be removed as a line and points calculated
                            {
                                rowsToRemove.Add(Convert.ToInt32(deadblock.y)); //Adds this line number to the rowsToRemove list
                            }
                        }
                        //Remove rows here
                        for (int i = 0; i < deadblocks.Count; i++) //Looping through every dead block
                        {
                            foreach (int row in rowsToRemove) 
                            {
                                if (row == (Convert.ToInt32(deadblocks[i].y))) //Checking if the dead block lies on one of the lines that should be removed, if so, deletes the block
                                {
                                    deadblocks.RemoveAt(i);
                                    deadblocktypes.RemoveAt(i);
                                    i -= 1; //Since we use RemoveAt, our current index automatically becomes the next block; Since the loop increments automatically we need to move index back; else we will skip a block
                                    break;
                                }
                            }
                        }

                        //Calculate what to move every row by
                        int[] moveBlocksBy = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //A list for the number of blocks/rows each row should be moved down for; e.g. when a line is removed the other blocks "drop"
                        for (int i = 0; i < moveBlocksBy.Length; i++)
                        {
                            //Counting how many rows under each row have been removed
                            int pushby = 0;
                            foreach (int row in rowsToRemove)
                            {
                                if (i < row)
                                {
                                    pushby++;
                                }
                            }
                            moveBlocksBy[i] = pushby; //Setting the number of the removed rows under current row, to the number of rows this row should be "dropped"/moved down by
                        }

                        //Move rows                        
                        for (int i = 0; i < deadblocks.Count; i++)
                        {
                            deadblocks[i].y += moveBlocksBy[Convert.ToInt32(deadblocks[i].y)]; //Moves the dead block down by the calculated number of rows
                        }

                        targetUpdateDelay -= rowsToRemove.Count * 2; //Speeds up the time based on how many lines you have cleared, more clears -> faster game!!

                        score += rowsToRemove.Count * rowsToRemove.Count * 10; //Calculates score addition based on number of lines cleared at once
                    }

                    currentBrick.baseCord.y += 1; //Move current brick down
                    clock.Restart(); //Resets update clock
                    updateDelay = targetUpdateDelay; //sets the new update delay (see line 221)
                }

                //Setting up the score
                Text scoreTxt = new Text(String.Concat("Score: ", score), scoreFont, 100);
                scoreTxt.Style = Text.Styles.Bold;
                scoreTxt.Color = Color.Black;

                scoreTxt.Scale = new Vector2f(0.35f, 0.35f);
                scoreTxt.Position = new Vector2f(10f, 420f);

                //Creating and adding the drawing sprites for each dead block to the sprite list
                foreach (Vector2 deadblock in deadblocks) //Add current "dead" block sprites
                {
                    int index = deadblocks.IndexOf(deadblock);
                    Vector2f posvec = new Vector2f(Convert.ToInt32(deadblock.x) * 18 + 28, Convert.ToInt32(deadblock.y) * 18 + 31);
                    Sprite spr = new Sprite(txt, new IntRect(deadblocktypes[index] * 18, 0, 18, 18));
                    spr.Position = posvec;
                    blocksprites.Add(spr);

                }

                //Creating and adding the sprites of the current tetrimino to the draw list
                foreach (Vector2 brickpiece in currentBrick.bricks) 
                {
                    Vector2f posvec = new Vector2f(Convert.ToInt32(brickpiece.x + currentBrick.baseCord.x)*18 + 28, Convert.ToInt32(brickpiece.y + currentBrick.baseCord.y) *18 + 31);
                    Sprite spr = new Sprite(txt, new IntRect(color*18, 0, 18, 18));
                    spr.Position = posvec;
                    bricksprites.Add(spr);
                }

                window.Draw(background); //Clears the screen and draw the background image on top

                //Draw every sprite
                foreach (Sprite sprite in bricksprites) 
                {
                    window.Draw(sprite);
                }

                foreach (Sprite sprite in blocksprites)
                {
                    window.Draw(sprite);
                }

                //Draws other cosmetic sprites such as frame and score
                window.Draw(frame);
                window.Draw(scoreTxt);

                //Displays what has been drawn to the buffer on the screen
                window.Display();
            }

        }

        static void OnClose(Object sender, EventArgs e) //If user is closing window -> close window
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        static void OnKeyPress(Object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Escape) //If user presses escape -> close window
            {
                RenderWindow window = (RenderWindow)sender;
                window.Close();
            }

            if (e.Code == Keyboard.Key.Space && cheatsOn) //If cheats have been activated, cycle the tetrimino/block type in real time each time user presses SPACE
            {
                if (type < 6)
                {
                    type += 1;
                } else
                {
                    type = 0;
                }
                color = type;

                currentBrick = new Brick(type, currentBrick.baseCord);
            }

            if (e.Code == Keyboard.Key.Left) //Moving the block to the left if the user wishes so
            {
                //Test if leftest block is out of play area {10, 20}
                movex = -1;
                bool insideblock = false;
                for (int i = 0; i < currentBrick.bricks.Length; i++)
                {
                    int absX = Convert.ToInt32(currentBrick.bricks[i].x + currentBrick.baseCord.x - 1); //Again, getting the global coordinates of each block in the brick/tetrimino after the tetrimino will be moved
                    int absY = Convert.ToInt32(currentBrick.bricks[i].y + currentBrick.baseCord.y);

                    foreach (Vector2 deadblock in deadblocks) //Checking if when the block is moved it doesn't intersect another dead block
                    {
                        if (absX == deadblock.x && absY == deadblock.y)
                        {
                            insideblock = true;
                        }
                    }

                    if (currentBrick.bricks[i].x + movex + currentBrick.baseCord.x < 0 || insideblock) //If it does intersect another block or is out of boundaries, nullify the movement the user wanted
                    {
                        movex = 0;
                    }
                }
            }

            if (e.Code == Keyboard.Key.Right) //The same as above, but for the other direction (to the right)
            {
                //Test if rightest block is out of play area {10, 20}
                movex = 1;
                bool insideblock = false;
                for (int i = 0; i < currentBrick.bricks.Length; i++)
                {
                    int absX = Convert.ToInt32(currentBrick.bricks[i].x + currentBrick.baseCord.x + 1);
                    int absY = Convert.ToInt32(currentBrick.bricks[i].y + currentBrick.baseCord.y);
                    foreach (Vector2 deadblock in deadblocks)
                    {
                        if (absX == deadblock.x && absY == deadblock.y)
                        {
                            insideblock = true;
                        }
                    }

                    if (currentBrick.bricks[i].x + movex + currentBrick.baseCord.x > 9 || insideblock)
                    {
                        movex = 0;
                    }
                }
            }

            if (e.Code == Keyboard.Key.Up && !gamePaused) //If user presses "UP" rotate block/tetrimino
            {
                //Creating a virtual bufferbrick that game will go back to if the current brick when rotated intersect or is out of border
                Brick bufferBrick = new Brick(currentBrick.type, currentBrick.baseCord); 
                for (int i = 0; i < 4; i++)
                {
                    bufferBrick.bricks[i] = currentBrick.bricks[i];
                }

                bool outOfBorder = false;
                bool insideBlock = false;

                //centerBlock is the coordinates to the very most centered block in the whole brick/tetrimino
                Vector2 centerBlock;
                if (currentBrick.type == 2 || currentBrick.type == 3 ||currentBrick.type == 0 || currentBrick.type == 4 || currentBrick.type == 5)
                {
                    centerBlock = currentBrick.bricks[1];
                } else if(currentBrick.type == 1) {
                    centerBlock = currentBrick.bricks[2];
                } else
                {
                    return;
                }

                //Rotating the brick by 90 degrees by actually finding the vector to each brick from the centerbrick and then setting it to the orthogonal vector of that
                for (int i = 0; i < currentBrick.bricks.Length; i++)
                {
                    Vector2 fromCenter = VectorFunctions.Difference(currentBrick.bricks[i], centerBlock);
                    fromCenter = VectorFunctions.Orthogonal(fromCenter);
                    currentBrick.bricks[i] = VectorFunctions.Sum(centerBlock, fromCenter);
                }

                //Checking if the rotation made the brick/tetrimino go out of border or intersect another dead block
                foreach (Vector2 block in currentBrick.bricks)
                {
                    int absX = Convert.ToInt32(block.x + currentBrick.baseCord.x);
                    int absY = Convert.ToInt32(block.y + currentBrick.baseCord.y);
                    if (absX < 0 || absX > 9)
                    {
                        outOfBorder = true;
                        break;
                    }

                    if (absY > 19)
                    {
                        outOfBorder = true;
                        break;
                    }

                    if (!outOfBorder)
                    {
                        foreach (Vector2 deadblock in deadblocks)
                        {
                            if (absX == deadblock.x && absY == deadblock.y)
                            {
                                insideBlock = true;
                            }
                        }
                    }
                }

                if (outOfBorder || insideBlock) //Going back to the bufferbrick checkpoint if current brick intersect or is out of border
                {
                    currentBrick = new Brick(bufferBrick.type, bufferBrick.baseCord);
                    for (int i = 0; i < 4; i++)
                    {
                        currentBrick.bricks[i] = bufferBrick.bricks[i];
                    }
                }

            }

            if (e.Code == Keyboard.Key.Down) //If holding down the DOWN key, increase block falling speed
            {
                updateDelay = 10;
            }

            if (e.Code == Keyboard.Key.P) //Pause game if P is pressed
            {
                gamePaused = !gamePaused;
            }

            if (cheatsOn == false) //Checking if the cheatcode "cheats" is being typed while the cheats are off
            {
                pressedKeys += e.Code.ToString();
                if (pressedKeys.ToLower().Contains("cheats"))
                {
                    cheatsOn = true;
                    pressedKeys = "";
                }

                //Making sure that tetrimino movement keys aren't added to this buffer
                pressedKeys.Replace(Keyboard.Key.Left.ToString(), "");
                pressedKeys.Replace(Keyboard.Key.Right.ToString(), "");
                pressedKeys.Replace(Keyboard.Key.Space.ToString(), "");
            }

        }

        static void OnResize(object sender, SizeEventArgs e) //Just making sure that the dimensions of the active game are stay the same when window is resized
        {
            RenderWindow window = (RenderWindow)sender;

            double new_Width = e.Height * 0.75;
            double new_Height = e.Width * 1.5;
            double new_Width_Ratio = new_Width / e.Width;
            double new_Height_Ratio = new_Height / e.Height;

            View view = window.GetView();

            if (new_Width <= e.Width)
            {
                view.Viewport = new FloatRect(0.0f, 0.0f, Convert.ToSingle(new_Width_Ratio), 1.0f);
            } else
            {
                view.Viewport = new FloatRect(0.0f, 0.0f, 1.0f, Convert.ToSingle(new_Height_Ratio));
            }
            
            window.SetView(view);
        }
    }
}
