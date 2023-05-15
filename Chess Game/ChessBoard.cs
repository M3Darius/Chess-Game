using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Game
{
	public class ChessBoard
	{
		public Bitmap BoardImage;
		public Point Center;
		public Point[] BoardCorners;
		public double InitialRotation;
		public double CurrentRotation;
		public double AngularVelocity;
		public double CameraAngle;
		public double CosCameraAngle;
		public float Width;
		public float Height;
		public float ScaleFactor;
		public string Player1SpriteSet;
		public string Player2SpriteSet;
		public int[,] BoardState; //matrice 8x8 care tine minte indicele din lista de piese
		public List<ChessPiece> Pieces;
		public ChessBoard(Point center, float scaleFactor, double initialRotation, double angularVelocity, double cameraAngle, string boardImageString = "board_black", string p1spriteset = "pieces_white", string p2spriteset = "pieces_black")
		{
			ResourceManager rm = Properties.Resources.ResourceManager;
			Center = center;
			ScaleFactor = scaleFactor;
			BoardImage = (Bitmap)rm.GetObject(boardImageString);

			Width = BoardImage.Width * ScaleFactor;
			Height = BoardImage.Height * ScaleFactor;

			BoardCorners = new Point[]
			{
				Point.Round(new PointF(-Width/2,-Height/ 2)),		// upper left
				Point.Round(new PointF(Width/2,-Height / 2)),		// upper right
				Point.Round(new PointF(-Width/2,Height / 2)),       // lower left
			};


			InitialRotation = initialRotation;
			CurrentRotation = initialRotation;
			AngularVelocity = angularVelocity;
			CameraAngle = cameraAngle;
			CosCameraAngle = Math.Cos(CameraAngle);
			Player1SpriteSet = p1spriteset;
			Player2SpriteSet = p2spriteset;

			Pieces = new List<ChessPiece>();
			BoardState = new int[8, 8];
		}
	}

}
