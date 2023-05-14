using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.IO;
using Chess_Game.Properties;

namespace Chess_Game
{
	public partial class Form1 : Form
	{
		public enum SpriteType
		{
			Pawn,
			Rook,
			Knight,
			Bishop,
			Queen,
			King,
			None
		};

		List<List<Bitmap>> PieceSprites = new List<List<Bitmap>>();
		Dictionary <string , int> PieceSpriteIndex = new Dictionary<string , int>();

		List<Bitmap> BoardSprites = new List<Bitmap>();
		Dictionary <string, int> BoardSpriteIndex = new Dictionary<string , int>();

		Rectangle[] CropAreas =
		{
			new Rectangle(0, 0, 15, 15),	//pawn
			new Rectangle(0, 15, 15, 15),	//rook
			new Rectangle(15, 15, 15, 15),	//knight
			new Rectangle(30, 15, 15, 15),	//bishop
			new Rectangle(45, 15, 15, 15),	//queen
			new Rectangle(60, 15, 15, 15),	//king
		};

		string[] BoardNames =
		{
			"board_black",
			"board_blue",
			"board_green",
			"board_pink",
			"board_wooden"
		};

		string[] PiecesNames =
		{
			"pieces_black",
			"pieces_blue",
			"pieces_green",
			"pieces_pink",
			"pieces_red",
			"pieces_violet",
			"pieces_white",
			"pieces_wooden_dark",
			"pieces_wooden_light",
			"pieces_yellow",

		};
		//setup, rotated 90 deg
		string[] InitialBoard =
		{
		"12345321",
		"00000000",
		"66666666",
		"66666666",
		"66666666",
		"66666666",
		"00000000",
		"12345321"
		};

		Random r = new Random();

		void SetupBoard()
		{
			for(int i = 0; i < InitialBoard.Length; i++)
				for(int j = 0; j < InitialBoard[i].Length; j++)
					if (InitialBoard[i][j] != '6')
						Pieces.Add(new ChessPiece(i, j, (SpriteType)Int32.Parse(InitialBoard[i][j].ToString()), PiecesNames[r.Next(0, 9)], Board));

		}
		void LoadImages()
		{
			ResourceManager rm = Resources.ResourceManager;

			foreach (string name in PiecesNames)
			{
					Bitmap spritesheet = (Bitmap)rm.GetObject(name);
					List<Bitmap> list = new List<Bitmap>();

					for(int i = 0; i <= 5; i++)
						list.Add(spritesheet.Clone(CropAreas[i], spritesheet.PixelFormat));
					PieceSpriteIndex.Add(name, PieceSprites.Count);
					PieceSprites.Add(list);

			}

			foreach (string name in BoardNames)
			{
				BoardSpriteIndex.Add(name, BoardSprites.Count);
				BoardSprites.Add((Bitmap)rm.GetObject(name));

			}
			
		}

		public class ChessPiece
		{
			public Tuple<int, int> PositionOnBoard;
			ChessBoard Board;
			public SpriteType Type;
			public string StyleName;
			public PointF RelativeCoords;

			public ChessPiece(int i, int j, SpriteType type, string styleName, ChessBoard board)
			{
				PositionOnBoard = new Tuple<int, int>(i, j);
				Type = type;
				StyleName = styleName;
				Board = board;
				RelativeCoords = CalcRelativeCoords(Board.RotationAngle);
			}
			public PointF CalcRelativeCoords(double theta) ///relative to center of the board
			{
				int i = PositionOnBoard.Item1, j = PositionOnBoard.Item2;

				return RotatePoint(
					new PointF(
					(float)(15 * (i - 4) + 8),
					(float)(-15 * (j - 3) + 5.5)
					), theta);

			}
		}

		

		List<ChessPiece> Pieces = new List<ChessPiece>();

		public class ChessBoard
		{
			public Bitmap BoardImage;
			public Point Center;
			public Point[] BoardCorners;
			public double RotationAngle;
			public double CameraAngle;
			public double CosCameraAngle;
			public float Width;
			public float Height;
			public float ScaleFactor;
			

			public ChessBoard(string boardImageString, Point center, float scaleFactor, double initialRotationAngle, double cameraAngle)
			{
				ResourceManager rm = Properties.Resources.ResourceManager;
				BoardImage = (Bitmap)rm.GetObject(boardImageString);
				Center = center;
				ScaleFactor = scaleFactor;

				Width = BoardImage.Width * ScaleFactor;
				Height = BoardImage.Height * ScaleFactor;

				BoardCorners = new Point[]	
				{
				Point.Round(new PointF(-Width/2,-Height/ 2)),		// upper left
				Point.Round(new PointF(Width/2,-Height / 2)),		// upper right
				Point.Round(new PointF(-Width/2,Height / 2)),		// lower left
				};

				RotationAngle = initialRotationAngle;
				CameraAngle = cameraAngle;
				CosCameraAngle = Math.Cos(CameraAngle);
			}
		}

		static ChessBoard Board;
		public Form1()
		{

			LoadImages();
			InitializeComponent();

			float scaleFactor = 5f;
			string boardname = BoardNames[r.Next(0, 4)];
			Point center = new Point(pictureBox1.Width/2, pictureBox1.Height/2);
			Board = new ChessBoard(boardname, center, scaleFactor, 0, Math.PI / 3);
			
			SetupBoard();

			AnimateRotation();
		}

		async void AnimateRotation()
		{
			while (true)
			{
				pictureBox1.Refresh();
				ChangeRotation(Board.RotationAngle + Math.PI / 180 / 3);
				await Task.Delay(50);
			}
		}
		
		static PointF RotatePoint(PointF originalPoint, double theta)
		{
			double sinTheta = Math.Sin(theta);
			double cosTheta = Math.Cos(theta);

			PointF newPoint = new PointF(
				(float)(originalPoint.X * cosTheta - originalPoint.Y * sinTheta),                               //X
				(float)((originalPoint.X * sinTheta + originalPoint.Y * cosTheta) * Board.CosCameraAngle));		//Y

			return newPoint;
		}

		Point[] GetSkewedCorners(Point[] originalPoints, double theta, Point offset)
		{
			double sinTheta = Math.Sin(theta);
			double cosTheta = Math.Cos(theta);

			Point[] newPoints = new Point[3];

			for (int i = 0; i < 3; i++)
			{
				newPoints[i].X = (int)(originalPoints[i].X * cosTheta - originalPoints[i].Y * sinTheta) + offset.X;
				newPoints[i].Y = (int)((originalPoints[i].X * sinTheta + originalPoints[i].Y * cosTheta) * Board.CosCameraAngle) + offset.Y;
			}

			return newPoints;
		}

		void DrawChessBoard(PaintEventArgs e, ChessBoard b)
		{
			//e.Graphics.DrawImage(boardImage, boardCorners);
			e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Point[] corners = GetSkewedCorners(b.BoardCorners, b.RotationAngle, b.Center);
			e.Graphics.DrawImage(b.BoardImage, corners);
		}

		void DrawChessPiece(PaintEventArgs e, ChessBoard b, ChessPiece cp)
		{
			e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			///asdasd
			PointF point = cp.RelativeCoords;

			float sizex = 65;
			float sizey = 65;

			RectangleF rect = new RectangleF
			(
				point.X * b.ScaleFactor - sizex / 2 + b.Center.X,
				point.Y * b.ScaleFactor - sizey / 2 - 4 * b.ScaleFactor + b.Center.Y,
				sizex, sizey
			);

			int imgIndex = PieceSpriteIndex[cp.StyleName];
			e.Graphics.DrawImage(PieceSprites[imgIndex][((int)cp.Type)], rect);

			//Pen p = new Pen(Brushes.Red, 5);
			//e.Graphics.DrawLine(p, b.Center, new PointF(point.X + b.Center.X, point.Y+b.Center.Y));
			//e.Graphics.FillRectangle(Brushes.Red, new RectangleF(new PointF(point.X + b.Center.X, point.Y + b.Center.Y), new SizeF(1, 1)));
			//e.Graphics.FillEllipse(Brushes.Red, point.X+b.Center.X - 2, point.Y+b.Center.Y-2, 4, 4);
		}

		void ChangeRotation(double theta)
		{
			Board.RotationAngle = theta;
			for(int i = 0; i < Pieces.Count; i++)
			{
				Pieces[i].RelativeCoords = Pieces[i].CalcRelativeCoords(theta);
			}
		}
		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			DrawChessBoard(e, Board);

			List<ChessPiece> ordered = Pieces.OrderBy(o => o.RelativeCoords.Y).ToList();

			foreach ( ChessPiece cp in ordered)
				DrawChessPiece(e, Board, cp);

		}

		private void pictureBox1_SizeChanged(object sender, EventArgs e)
		{
			Board.Center = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
		}
	}
}
