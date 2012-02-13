using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SaturnIV
{
    public class Camera
    {
        public enum CameraMode
        {
            free = 0,
            chase = 1,
            orbit = 2
        }
        public static CameraMode currentCameraMode = CameraMode.chase;

        float mousemoveX = 0;
        float mousemoveY = 0;

        // This float holds the Angle the camera has moved around the Z.axis since last frame.
        float AngleAddZ = 0;

        // This float holds the Angle the camera has moved around the Y.axis since last frame.
        float AngleAddY = 0;


        public static Vector3 position;
        private Vector3 desiredPosition;
        private Vector3 target;
        private Vector3 desiredTarget;
        public static Vector3 offsetDistance;

        private float yaw, pitch, roll;
        private float speed;
        public static float zoomFactor = 4f;
        private int mscreenMiddleX, mscreenMiddleY;

        public Matrix cameraRotation;
        public Matrix viewMatrix, projectionMatrix;

        private MouseState mouseStateCurrent;
        private MouseState mouseStatePrevious;
        private MouseState originalMouseState;
        private KeyboardState keyboardStatePrevious;

        public Camera(int screenMiddleX,int screenMiddleY)
        {
            ResetCamera();
            mscreenMiddleX = screenMiddleX; mscreenMiddleY = screenMiddleY;
            Mouse.SetPosition(mscreenMiddleX,mscreenMiddleY);
            originalMouseState = Mouse.GetState();
        }

        public void ResetCamera()
        {
            position = new Vector3(10, 10, 1);
            desiredPosition = position;
            target = new Vector3();
            desiredTarget = target;
            //offsetDistance = new Vector3(-5400, 2200, 250);
            //offsetDistance = new Vector3(0, 0, 50);

            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;

            speed = .6f;

            cameraRotation = Matrix.Identity;
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(65.0f), 4.0f / 3.0f, 0.5f, 120000f);       
        }

        public void Update(Matrix chasedObjectsWorld, Vector3 playerUp,Vector3 playerForward)
        {
            HandleInput();
            UpdateViewMatrix(chasedObjectsWorld,playerUp,playerForward);
        }

        private void HandleInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (mouseStateCurrent.ScrollWheelValue > mouseStatePrevious.ScrollWheelValue)
            {
                float WheelVal = (mouseStateCurrent.ScrollWheelValue -
                             mouseStatePrevious.ScrollWheelValue) / 120;
                //if (zoomFactor < 25.0)
                zoomFactor += (WheelVal * 0.15f);
            }

            //! Scroll-Down | Zoom Out
            if (mouseStateCurrent.ScrollWheelValue < mouseStatePrevious.ScrollWheelValue)
            {
                float WheelVal = (mouseStateCurrent.ScrollWheelValue -
                             mouseStatePrevious.ScrollWheelValue) / 120;

                //if (zoomFactor > 0.50)
                zoomFactor -= (WheelVal * -0.15f);
            }


            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                position.Y = 42500;
            }
            mouseStatePrevious = mouseStateCurrent;
            keyboardStatePrevious = keyboardState;
        }

        private void MoveCamera(Vector3 addedVector)
        {
            position += speed * addedVector * zoomFactor;
        }

        private void UpdateViewMatrix(Matrix chasedObjectsWorld, Vector3 playerUp, Vector3 playerForward)
        {
            switch (currentCameraMode)
            {
                case CameraMode.free:

                    cameraRotation.Forward.Normalize();
                    cameraRotation.Up.Normalize();
                    cameraRotation.Right.Normalize();

                    cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Right, pitch);
                    cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Up, yaw);
                    cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);

                    yaw = 0.0f;
                    pitch = 0.0f;
                    roll = 0.0f;

                    target = position + cameraRotation.Forward;
                    
                    break;

                case CameraMode.chase:

                    cameraRotation.Forward.Normalize();
                    chasedObjectsWorld.Right.Normalize();
                    chasedObjectsWorld.Up.Normalize();

                    cameraRotation = Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);
                    
                    desiredTarget = chasedObjectsWorld.Translation;
                    target = desiredTarget;

                    target += chasedObjectsWorld.Right * yaw;
                    target += chasedObjectsWorld.Up * pitch;

                    Math.Abs(zoomFactor);
                    desiredPosition = Vector3.Transform(offsetDistance, chasedObjectsWorld) * zoomFactor;
                    position = Vector3.SmoothStep(position, desiredPosition, .25f);
                    
                    yaw = MathHelper.SmoothStep(yaw, 0f, .1f);
                    pitch = MathHelper.SmoothStep(pitch, 0f, .1f);
                    roll = MathHelper.SmoothStep(roll, 0f, .2f);

                    break;

                case CameraMode.orbit:

                    cameraRotation.Forward.Normalize();
                  //  offsetDistance = offsetDistance;
                    cameraRotation = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw) 
                            * Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);
                    desiredPosition = Vector3.Transform(offsetDistance, cameraRotation);
                    desiredPosition += chasedObjectsWorld.Translation;
                    position = desiredPosition;
                    target = chasedObjectsWorld.Translation;
                    roll = MathHelper.SmoothStep(roll, 0f, .2f);                                                         
                    break;
            }

            //We'll always use this line of code to set up the View Matrix.
            if (currentCameraMode == CameraMode.orbit)
                viewMatrix = Matrix.CreateLookAt(position, target,new Vector3(0, 1, 0));
            else
                viewMatrix = Matrix.CreateLookAt(chasedObjectsWorld.Translation,
                                              chasedObjectsWorld.Translation + playerForward,
                                              playerUp);
        }

        //This cycles through the different camera modes.
        public void SwitchCameraMode()
        {
            ResetCamera();

            currentCameraMode++;

            if ((int)currentCameraMode > 2)
            {
                currentCameraMode = 0;
            }
        }
    }
}

