using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace GAOC1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D backgroundTexture;
        Texture2D backgroundMenuTexture;
        Rectangle viewportRect;

        int spelen = 0;

        #region cannon&cannonballs
        GameObject cannon;

        int maxCannonBalls = 3;
        GameObject[] cannonBalls;
        KeyboardState previousKeyBoardState = Keyboard.GetState();
        #endregion

        #region enemies
        int maxEnemies = 3;
        GameObject[] enemies;
        const float maxEnemyHeight = 0.1f;
        const float minEnemyHeight = 0.5f;
        const float maxEnemyVelocity = 5.0f;
        const float minEnemyVelocity = 1.0f;
        Random random = new Random();
        int enemynummer = 0;
        #endregion

        #region sound

        AudioEngine audioEngine;
        SoundBank soundbank;
        WaveBank wavebank;

        #endregion

        #region explosion

        Texture2D spriteSheet;

        Explosion[] explosions;

        Rectangle sourceRect;
        Rectangle destinationRect;

        float timer = 0f;
        float interval = 1000f / 25f;
        int frameCount = 16;
        int currentFrame = 0;
        int spriteWidth = 64;
        int spriteHeight = 64;
        #endregion

        #region score
        SpriteFont font;
        Vector2 scoreDrawPoint = new Vector2(0.1f, 0.1f);
        #endregion

        int score;

        int CheatsEnabled = 0;


        Color[] cannonBallTextureData;
        Color[] enemyTextureData;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            #region Background
            backgroundTexture = Content.Load<Texture2D>("Sprites\\background");
            backgroundMenuTexture = Content.Load<Texture2D>("Menu\\titel");
            viewportRect = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);
            #endregion

            #region cannon
            cannon = new GameObject(Content.Load<Texture2D>("Sprites\\cannon"));
            cannon.position = new Vector2(120, graphics.GraphicsDevice.Viewport.Height - 80);
            #endregion

            #region cannonBalls
            if (CheatsEnabled == 0)
            {
                cannonBalls = new GameObject[maxCannonBalls];
                for (int i = 0; i < maxCannonBalls; i++)
                {
                    cannonBalls[i] = new GameObject(Content.Load<Texture2D>("Sprites\\cannonball"));
                }
            }

            cannonBallTextureData = new Color[cannonBalls[0].sprite.Width * cannonBalls[0].sprite.Height];
            cannonBalls[0].sprite.GetData(cannonBallTextureData);

            #endregion

            #region enemies
            enemies = new GameObject[maxEnemies];
            explosions = new Explosion[maxEnemies];
            for (int i = 0; i < maxEnemies; i++)
            {
                enemies[i] = new GameObject(Content.Load<Texture2D>("Sprites\\enemy"));
                explosions[i] = new Explosion(Content.Load<Texture2D>("Sprites\\sprite_sheet"));
            }

            enemyTextureData = new Color[enemies[0].sprite.Width * enemies[0].sprite.Height];
            enemies[0].sprite.GetData(enemyTextureData);

            #endregion

            #region sound

            audioEngine = new AudioEngine("Content\\Audio\\audio.xgs");
            wavebank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");
            soundbank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");

            #endregion

            spriteSheet = Content.Load<Texture2D>("Sprites\\sprite_sheet");
            destinationRect = new Rectangle(0, 0, spriteWidth, spriteHeight);

            font = Content.Load<SpriteFont>("Fonts\\GameFont");

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (spelen == 1)
            {
                UpdatePlay(gameTime);
                DrawPlay(gameTime);
            }
            else
            {
                UpdateMenu(gameTime);
                DrawMenu(gameTime);
            }
        }

        public void UpdatePlay(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            #region Cheats

            if (keyboardState.IsKeyDown(Keys.L))
            {
                CheatsEnabled = 1;
                score += 1000;
                maxCannonBalls = 150;
                cannonBalls = new GameObject[maxCannonBalls];
                for (int i = 0; i < maxCannonBalls; i++)
                {
                    cannonBalls[i] = new GameObject(Content.Load<Texture2D>("Sprites\\cannonball"));
                }
            }

            if(CheatsEnabled == 1)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    FireCannonBall();
                }
            }


            #endregion

            #region KeyLeft
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                cannon.rotation -= 0.1f;
            }
            #endregion

            #region KeyRight
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                cannon.rotation += 0.1f;
            }
            #endregion

            #region KeyEscape
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            #endregion

            #region KeySpace 
            if (CheatsEnabled == 0)
            {
                if (keyboardState.IsKeyDown(Keys.Space) && previousKeyBoardState.IsKeyUp(Keys.Space))
                {
                    FireCannonBall();
                }
            }
            #endregion

            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if(timer > interval)
            {
                currentFrame++;
                if (currentFrame > frameCount - 1)
                {
                    currentFrame = 0;
                }

                timer = 0f;

            }

            UpdateExplosions((float)gameTime.ElapsedGameTime.TotalMilliseconds);

            sourceRect = new Rectangle(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);

            previousKeyBoardState = keyboardState;

            UpdateCannonBalls();
            UpdateEnemies();

            cannon.rotation = MathHelper.Clamp(cannon.rotation, -MathHelper.PiOver2, 0);
            base.Update(gameTime);
        }

        public void UpdateMenu(GameTime gameTime)
        {

            KeyboardState keyboardState = Keyboard.GetState();

            #region ToetsA
            if (keyboardState.IsKeyDown(Keys.A))
            {
                maxEnemies = 3;
                spelen = 1;
                LoadEnemies();
            }
            #endregion

            #region ToetsB
            if (keyboardState.IsKeyDown(Keys.B))
            {
                maxEnemies = 6;
                spelen = 1;
                LoadEnemies();
            }
            #endregion

            #region ToetsC
            if (keyboardState.IsKeyDown(Keys.C))
            {
                maxEnemies = 9;
                spelen = 1;
                LoadEnemies();
            }
            #endregion

            base.Update(gameTime);
        }

        public void DrawPlay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, viewportRect, Color.White);


            #region DrawCannonBalls
            foreach(GameObject ball in cannonBalls)
            {
                if (ball.alive)
                {
                    spriteBatch.Draw(ball.sprite, ball.position, Color.White);
                }

            }
            #endregion

            #region DrawEnemy
            foreach(GameObject enemy in enemies)
            {
                if(enemy.alive)
                {
                    spriteBatch.Draw(enemy.sprite, enemy.position, Color.White);
                }
            }
            #endregion

            #region Score
            spriteBatch.DrawString(font,
                "Score: " + score.ToString(),
                new Vector2(scoreDrawPoint.X * viewportRect.Width,
                    scoreDrawPoint.Y * viewportRect.Height),
                    Color.Yellow);
            #endregion

            #region DrawCheats
            if (CheatsEnabled == 1)
            {
                spriteBatch.DrawString(font,
                    "Cheats Enabled",
                    new Vector2(300, 48),
                        Color.Yellow);
            }
            #endregion

            #region explosion
            foreach (Explosion explosion in explosions)
            {
                if (explosion.alive)
                {
                    spriteBatch.Draw(explosion.sprite,
                    explosion.position, explosion.sourceRect,
                    Color.White, 0f, Vector2.Zero, 1.5f,
                    SpriteEffects.None, 0);
                }
            }
            #endregion

            #region DrawCannon
                spriteBatch.Draw(cannon.sprite,
                    cannon.position,
                    null,
                    Color.White,
                    cannon.rotation,
                    cannon.center, 1.0f,
                    SpriteEffects.None, 0);
            #endregion

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawMenu(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(backgroundMenuTexture, viewportRect, Color.White);

            spriteBatch.End();
        }

        public void FireCannonBall()
        {
            foreach (GameObject ball in cannonBalls)
            {
                if (!ball.alive)
                {
                    ball.alive = true;
                    ball.position = cannon.position - ball.center;
                    ball.velocity = new Vector2(
                        (float)Math.Cos(cannon.rotation),
                        (float)Math.Sin(cannon.rotation)) * 5.0f;
                    soundbank.PlayCue("missilelaunch");
                    return;
                }
            }
        }

        public void UpdateCannonBalls()
        {
            foreach (GameObject ball in cannonBalls)
            {
                if (ball.alive)
                {
                    ball.position += ball.velocity;
                    if(!viewportRect.Contains(new Point(
                        (int)ball.position.X,
                        (int)ball.position.Y)))
                    {
                        ball.alive = false;
                        continue;
                    }

                    Rectangle cannonBallRect = new Rectangle(
                        (int)ball.position.X,
                        (int)ball.position.Y,
                        ball.sprite.Width,
                        ball.sprite.Height);

                    

                    foreach(GameObject enemy in enemies)
                    {
                        
                        Rectangle enemyRect = new Rectangle(
                            (int)enemy.position.X,
                            (int)enemy.position.Y,
                            enemy.sprite.Width,
                            enemy.sprite.Height);

                        if (IntersectPixels(cannonBallRect, cannonBallTextureData,
                                    enemyRect, enemyTextureData))
                        {
                            ball.alive = false;
                            enemy.alive = false;
                            score += 1;

                            if (enemynummer >= 2)
                            {
                                enemynummer = 0;
                            }
                            else
                            {
                                enemynummer++;
                            }
                            explosions[enemynummer].alive = true;
                            explosions[enemynummer].timer = 0;
                            explosions[enemynummer].currentFrame = 0;
                            explosions[enemynummer].position = enemy.position;
                            soundbank.PlayCue("explosion");
                            
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateExplosions(float timertijd)
        {
            foreach (Explosion explosion in explosions)
            {
                if (explosion.alive)
                {
                    explosion.timer += timertijd;
                    if (explosion.timer > interval)
                    {
                        explosion.currentFrame++;
                        if (explosion.currentFrame > frameCount - 1)
                        {
                            explosion.alive = false;
                        }
                        explosion.timer = 0f;
                    }
                    explosion.sourceRect = new
                    Rectangle(explosion.currentFrame * spriteWidth,
                    0, spriteWidth, spriteHeight);
                }
            }
        }

        public void UpdateEnemies()
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy.alive)
                {
                    enemy.position += enemy.velocity;
                    if (!viewportRect.Contains(new Point((int)enemy.position.X, (int)enemy.position.Y)))
                    {
                        enemy.alive = false;
                    }
                }
                else
                {
                    enemy.alive = true;
                    enemy.position = new Vector2(
                        viewportRect.Right,
                        MathHelper.Lerp(
                        (float)viewportRect.Height * minEnemyHeight,
                        (float)viewportRect.Height * maxEnemyHeight,
                        (float)random.NextDouble()));
                    enemy.velocity = new Vector2(
                        MathHelper.Lerp(
                        -minEnemyVelocity,
                        -maxEnemyVelocity,
                        (float)random.NextDouble()), 0);
                }
            }
        }

        static bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                    Rectangle rectangleB, Color[] dataB)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    Color colorA = dataA[(x - rectangleA.Left) +
                                         (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = dataB[(x - rectangleB.Left) +
                                         (y - rectangleB.Top) * rectangleB.Width];

                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }

        public void LoadEnemies()
        {
            #region enemies
            enemies = new GameObject[maxEnemies];
            explosions = new Explosion[maxEnemies];
            for (int i = 0; i < maxEnemies; i++)
            {
                enemies[i] = new GameObject(Content.Load<Texture2D>("Sprites\\enemy"));
                explosions[i] = new Explosion(Content.Load<Texture2D>("Sprites\\sprite_sheet"));
            }

            enemyTextureData = new Color[enemies[0].sprite.Width * enemies[0].sprite.Height];
            enemies[0].sprite.GetData(enemyTextureData);

            #endregion
        }

    }
}
