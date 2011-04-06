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
        public CameraMode currentCameraMode = CameraMode.orbit;

        public Vector3 position;
        private Vector3 desiredPosition;
        private Vector3 target;
        private Vector3 desiredTarget;
        public Vector3 offsetDistance;

        private float yaw, pitch, roll;
        private float speed;
        private float zoomFactor = 0.25f;
        private int mscreenMiddleX, mscreenMiddleY;

        public Matrix cameraRotation;
        public Matrix viewMatrix, projectionMatrix;

        private MouseState mouseStateCurrent;
        private MouseState mouseStatePrevious;
        private MouseState originalMouseState;

        public Camera(int screenMiddleX,int screenMiddleY)
        {
            ResetCamera();
            mscreenMiddleX = screenMiddleX; mscreenMiddleY = screenMiddleY;
            Mouse.SetPosition(mscreenMiddleX,mscreenMiddleY);
            originalMouseState = Mouse.GetState();
        }

        public void ResetCamera()
        {
            position = new Vector3(0, 0, 50);
            desiredPosition = position;
            target = new Vector3();
            desiredTarget = target;
            offsetDistance = new Vector3(0.0f, 1000.0f, 500.0f);
            //offsetDistance = new Vector3(0, 0, 50);

            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;

            speed = .3f;

            cameraRotation = Matrix.Identity;
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75.0f), 4.0f / 3.0f, 2.5f, 10000f);       
        }

        public void Update(Matrix chasedObjectsWorld)
        {
            HandleInput();
            UpdateViewMatrix(chasedObjectsWorld);
        }

        private void HandleInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            mouseStateCurrent = Mouse.GetState();

            if (mouseStateCurrent.ScrollWheelValue > mouseStatePrevious.ScrollWheelValue)
            {
                float WheelVal = (mouseStateCurrent.ScrollWheelValue -
                             mouseStatePrevious.ScrollWheelValue) / 120;
                if (zoomFactor < 2.0)
                zoomFactor += (WheelVal * 0.025f);
            }

            //! Scroll-Down | Zoom Out
            if (mouseStateCurrent.ScrollWheelValue < mouseStatePrevious.ScrollWheelValue)
            {
                float WheelVal = (mouseStateCurrent.ScrollWheelValue -
                             mouseStatePrevious.ScrollWheelValue) / 120;
                
                 if (zoomFactor >0.10)
                     zoomFactor -= (WheelVal * -0.025f);
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (mouseStateCurrent != originalMouseState)
                {
                    float xDifference = mouseStateCurrent.X - originalMouseState.X;
                    float yDifference = mouseStateCurrent.Y - originalMouseState.Y;
                    yaw -=  (xDifference/2) * .01f;
                    pitch -= (yDifference / 2) * .01f;
                    Mouse.SetPosition(mscreenMiddleX, mscreenMiddleY);
                }
            }
            mouseStatePrevious = mouseStateCurrent;
        }

        private void MoveCamera(Vector3 addedVector)
        {
            position += speed * addedVector * zoomFactor;
        }

        private void UpdateViewMatrix(Matrix chasedObjectsWorld)
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
                    
                    desiredPosition = Vector3.Transform(offsetDistance, chasedObjectsWorld);
    
                    position = Vector3.SmoothStep(position, desiredPosition, .15f);
                    
                    yaw = MathHelper.SmoothStep(yaw, 0f, .1f);
                    pitch = MathHelper.SmoothStep(pitch, 0f, .1f);
                    roll = MathHelper.SmoothStep(roll, 0f, .2f);

                    break;

                case CameraMode.orbit:

                    cameraRotation.Forward.Normalize();
                  //  offsetDistance = offsetDistance;
                    cameraRotation = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw) * Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);

                    desiredPosition = Vector3.Transform(offsetDistance, cameraRotation) * zoomFactor;
                    desiredPosition += chasedObjectsWorld.Translation;
                    position = desiredPosition;

                    target = chasedObjectsWorld.Translation;

                    roll = MathHelper.SmoothStep(roll, 0f, .2f);                                                         


                    break;
            }

            //We'll always use this line of code to set up the View Matrix.
            viewMatrix = Matrix.CreateLookAt(position, target,new Vector3(0, 1, 0));
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

