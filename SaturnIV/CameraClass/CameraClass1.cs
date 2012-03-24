using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SaturnIV
{
    public class CameraNew
    {
        public enum CameraMode
        {
            free = 0,
            chase = 1,
            orbit = 2
        }
        public static CameraMode currentCameraMode = CameraMode.free;

        public static Vector3 position;
        private Vector3 desiredPosition;
        private Vector3 target;
        private Vector3 desiredTarget;
        public static Vector3 offsetDistance;

        private float yaw, pitch, roll;
        private float speed;

        Matrix cameraRotation;
        public Matrix viewMatrix, projectionMatrix;

        private MouseState mouseStateCurrent;
        private MouseState mouseStatePrevious;
        private MouseState originalMouseState;
        public static float zoomFactor = 12.5f;
        float preZoomFactor;

        public CameraNew()
        {
            int screenMiddleX = 1280 / 2;
            int screenMiddleY = 1024 / 2;
            ResetCamera();
            int mscreenMiddleX = screenMiddleX; 
            int mscreenMiddleY = screenMiddleY;
            Mouse.SetPosition(mscreenMiddleX, mscreenMiddleY);
            originalMouseState = Mouse.GetState();
        }

        public void ResetCamera()
        {
            position = new Vector3(0, 0, 50);
            desiredPosition = position;
            target = new Vector3();
            desiredTarget = target;

            offsetDistance = new Vector3(0, 500, 50);

            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;

            speed = .3f;

            cameraRotation = Matrix.Identity;
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60.0f), 4 / 3, 1.5f, 6700000f);
        }

        public void Update(Matrix chasedObjectsWorld, bool isEditMode)
        {
            HandleInput(isEditMode);
            UpdateViewMatrix(chasedObjectsWorld);
        }

        private void HandleInput(bool isEditMode)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            mouseStateCurrent = Mouse.GetState();
            if (mouseStateCurrent.ScrollWheelValue > mouseStatePrevious.ScrollWheelValue)
            {
                float WheelVal = (mouseStateCurrent.ScrollWheelValue -
                             mouseStatePrevious.ScrollWheelValue) / 120;
                if (zoomFactor < 35.0)
                zoomFactor += (WheelVal * 1.25f);
            }

            //! Scroll-Down | Zoom Out
            if (mouseStateCurrent.ScrollWheelValue < mouseStatePrevious.ScrollWheelValue)
            {
                float WheelVal = (mouseStateCurrent.ScrollWheelValue -
                             mouseStatePrevious.ScrollWheelValue) / 120;

                if (zoomFactor > 6.0)
                zoomFactor -= (WheelVal * -1.25f);
            }

            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState && !isEditMode && Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                //leftrightRot -= rotationSpeed * xDifference * amount;
                pitch += .005f * yDifference * speed;
                yaw -= .005f * xDifference * speed;
                
            }
            mouseStatePrevious = mouseStateCurrent;
        }

        private void MoveCamera(Vector3 addedVector)
        {
            position += speed * addedVector;
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

                    desiredPosition = Vector3.Transform(offsetDistance, chasedObjectsWorld) * zoomFactor;
                    position = Vector3.SmoothStep(position, desiredPosition, .15f);

                    yaw = MathHelper.SmoothStep(yaw, 0f, .1f);
                    pitch = MathHelper.SmoothStep(pitch, 0f, .1f);
                    roll = MathHelper.SmoothStep(roll, 0f, .2f);

                    break;

                case CameraMode.orbit:

                    cameraRotation.Forward.Normalize();

                    cameraRotation = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw) * Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);

                    desiredPosition = Vector3.Transform(offsetDistance, cameraRotation) * zoomFactor;
                    desiredPosition += chasedObjectsWorld.Translation;
                    position = desiredPosition;
                    target = chasedObjectsWorld.Translation;

                    roll = MathHelper.SmoothStep(roll, 0f, .2f);

                    break;
            }

            //We'll always use this line of code to set up the View Matrix.
            viewMatrix = Matrix.CreateLookAt(position, target, cameraRotation.Up);
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

