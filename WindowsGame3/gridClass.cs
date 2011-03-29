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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace SaturnIV
{

	    public class Grid 
 	    { 
	        private int interval; 
	        private int points; 
            private int side; 
	        private VertexPositionColor[] pointList; 
 	        private int[] lineListIndices; 
 	        private float posX; 
 	        private float posZ; 
 	        private float posY = -400f; 
 	        private GraphicsDevice graphicsDevice;
            Effect effect;
 	 
 	        /// <summary> 
 	        /// Constructor 
 	        /// </summary> 
 	        /// <param name="side">Number of points per side</param> 
 	        /// <param name="interval">Space between points</param> 
 	        /// <param name="graphicsDevice">Instance of GraphicsDevice</param> 
 	        /// <param name="game">Instance of Game</param> 
 	        public Grid(int side, int interval, GraphicsDevice graphicsDevice, Game game) 
 	        { 
 	            //maximum side size is 700 
 	            side = side > 700 ? 700 : side; 
 	 
 	            this.side = side; 
 	            this.points = side * side; 
 	            this.interval = interval; 
 	            this.pointList = new VertexPositionColor[points]; 
 	            this.lineListIndices = new int[points * 4]; 
 	            this.posX = (side * interval) / 2; 
 	            this.posZ = (side * interval) / 2; 
 	            this.graphicsDevice = graphicsDevice;
 	        } 
 	 
 	        /// <summary> 
 	        /// Populate vertices array 
 	        /// </summary> 
 	        private void InitializePoints() 
 	        { 
 	 
 	            for (int x = 0; x < this.side; x++) 
 	            { 
 	                for (int y = 0; y < this.side; y++) 
 	                { 
 	                     pointList[(x * this.side) + y] = new VertexPositionColor(new Vector3((x * this.interval)-this.posX, this.posY, (y * this.interval)-this.posZ), Color.Red); 
 	                } 
 	            } 
 	        } 
 	 
 	        /// <summary> 
 	        /// Draw lines between extremities 
 	        /// </summary> 
 	        private void InitializeLines() 
 	        { 
 	            this.InitializePoints(); 
 	 
 	            for (int i = 0; i < this.side; i++) 
 	            { 
 	                lineListIndices[(i * 4) + 0] = i; 
 	                lineListIndices[(i * 4) + 1] = ((this.side - 1) * this.side) + i; 
 	                lineListIndices[(i * 4) + 2] = i * this.side; 
 	                lineListIndices[(i * 4) + 3] = ((i+1) * this.side) - 1; 
 	            } 
 	 
 	 
 	 
 	        } 
 	 
 	        /// <summary> 
 	        /// Number of points 
 	        /// </summary> 
 	        /// <returns>points</returns> 
 	        public int getNumberOfPoints() 
 	        { 
 	            return this.points; 
 	        } 
 	 
 	        /// <summary> 
 	        /// Set the Height of the grid 
 	        /// </summary> 
 	        /// <param name="y">Hauteur</param> 
 	        public void setPositionY(float y) 
 	        { 
 	            this.posY = y; 
 	        } 
 	 
 	        /// <summary> 
 	        /// Get vertices array 
 	        /// </summary> 
 	        /// <returns>Vertices</returns> 
 	        public VertexPositionColor[] getVertices() 
 	        { 
 	            return this.pointList; 
 	        } 
 	 
 	        /// <summary> 
 	        /// Returns the lines' reference array 
 	        /// </summary> 
 	        /// <returns>Reference array</returns> 
 	        public int[] getLineIndices() 
 	        { 
 	            return this.lineListIndices; 
 	        } 
 	 
 	        /// <summary> 
 	        /// Draws the grid with lines 
 	        /// </summary> 
 	        public void drawLines() 
 	        { 
 	            this.InitializeLines(); 
 	            
 	            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>( 
 	                                PrimitiveType.LineList, 
 	                                this.getVertices(), 
 	                                0, 
 	                                this.getNumberOfPoints(), 
 	                                this.getLineIndices(), 
 	                                0, 
	                                this.side * 4 
	                                ); 
	                     
	        } 
	 
	        /// <summary> 
        /// Draws the grid with dots 
	        /// </summary> 
	        /// <param name="pointSize">Dot size in pixels</param> 
	        public void drawPoints(int pointSize) 
        { 
	             
	            graphicsDevice.RenderState.PointSize = pointSize; 
	            this.InitializePoints(); 
	 
	            graphicsDevice.DrawUserPrimitives<VertexPositionColor>( 
 	                        PrimitiveType.PointList, 
	                        this.getVertices(), 
	                        0, 
                        this.getNumberOfPoints() 
	                        ); 
	 
	        }
 	 
	    } 
} 
	 