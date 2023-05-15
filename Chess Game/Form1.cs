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
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Chess_Game
{
	public partial class Form1 : Form
	{
		ResourceManager rm = Resources.ResourceManager;
		Bitmap Shadow;
		Bitmap Circle;
		bool PieceIsSelected = false;
		bool CanSelect = false;
		string CurrentPlayer = "Player1";

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

		static public string[] BoardNames =
		{
			"board_black",
			"board_blue",
			"board_green",
			"board_purple",
			"board_wooden"
		};

		static public string[] PiecesNames =
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

		void InitTimerLabel()
		{
			/**
			PrivateFontCollection pfc = new PrivateFontCollection();
			string filename = @"../../Resources/Minecraft.ttf";
			pfc.AddFontFile(filename);
			**/
			label1.BackColor = Color.FromArgb(253, 254, 250);
			//label1.Font = new Font(pfc.Families[0], label1.Font.Size);
			label1.Text = "0:00";

			//pfc.Dispose();
		}

		void SetColors(int p1, int p2, int board)
		{
			Board.BoardImage = BoardSprites[board];
			Board.Player1SpriteSet = PiecesNames[p1];
			Board.Player1SpriteSet = PiecesNames[p2];
		}
		void RandomColors()
		{
			int k = r.Next(BoardNames.Length);
			Board.BoardImage = BoardSprites[k];

			int a, b;
			a = r.Next(PiecesNames.Length);
			Board.Player1SpriteSet = PiecesNames[a];
			do
			{
				b = r.Next(PiecesNames.Length);
			} while (a == b);
			Board.Player2SpriteSet = PiecesNames[b];
		}
		void SetupBoard(int p1, int p2, int board)
		{
			float scaleFactor = 5f;
			string boardname = BoardNames[r.Next(0, 4)];
			Point center = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);

			double initialRot = Math.PI / 180 * 185;
			double angVec = Math.PI / 180 * 0;
			double camAngleToNormal = Math.PI / 180 * 60;
			Board = new ChessBoard(center, scaleFactor, initialRot, angVec, camAngleToNormal);
			SetColors(p1, p2, board);


			for (int i = 0; i < InitialBoard.Length; i++)
				for(int j = 0 ; j < InitialBoard[i].Length ; j++)
					if (InitialBoard[i][j] != '6')
					{
						string owner = i <= 4 ? "Player1" : "Player2";
						string spriteset = i <= 4 ? Board.Player1SpriteSet : Board.Player2SpriteSet;
						var x = (SpriteType)Int32.Parse(InitialBoard[i][j].ToString());
						ChessPiece piece = new ChessPiece(i, j, x, Board, owner, spriteset);
						Board.BoardState[i, j] = Board.Pieces.Count;
						Board.Pieces.Add(piece);
					}
					else
					{
						Board.BoardState[i, j] = -1;
					}

		}
		void LoadImages()
		{
			ResourceManager rm = Resources.ResourceManager;

			Shadow = (Bitmap)rm.GetObject("shadow");
			Circle = (Bitmap)rm.GetObject("circle");

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

		static ChessBoard Board;
		public Form1(int p1, int p2, int board)
		{
			LoadImages();
			InitializeComponent();
			InitTimerLabel();

			CurrentPlayer = "Player1";
			timer1.Start();
			
			SetupBoard(p1, p2, board);
			AnimateRotation();
		}

		Stopwatch sw = new Stopwatch();
		double TimeStartBoardRotation = 0;
		double AccumulatedAngle = Math.PI/180*15;
		double SmoothStep(double x, double min = 0, double max = 1)
		{
			if (x < min)
				return min;
			if(x > max) 
				return max;
			return x * x * (3 - 2 * x);
		}

		async void AnimateRotation()
		{
			sw.Start();

			while (true)
			{
				pictureBox1.Refresh();

				//ChangeRotation(CurrentPlayer == "Player1" ? Math.PI/180 * 185 : Math.PI/180 * 5);

				double dt = sw.ElapsedMilliseconds - TimeStartBoardRotation;
				//ChangeRotation(SmoothStep(dt/10000) * (CurrentPlayer == "Player1" ? Math.PI: Math.PI*2) + Math.PI/180*5);
				ChangeRotation(AccumulatedAngle + Math.PI * SmoothStep(dt/2500));
				if (dt >= 2500)
					CanSelect = true;
				
				/**
				Board.CameraAngle = Board.CameraAngle + Math.PI / 180/4;
				Board.CosCameraAngle = Math.Cos(Board.CameraAngle);
				**/
				await Task.Delay(15);
			}
		}
		public static PointF ProjectVectorToPlane(PointF point, double costheta)
		{
			PointF newPoint = new PointF(
				point.X,
				point.Y * (float)costheta
				);

			return newPoint;
		}
		public static PointF RotateVector(PointF originalPoint, double theta)
		{
			double sinTheta = Math.Sin(theta);
			double cosTheta = Math.Cos(theta);

			PointF newPoint = new PointF(
				(float)(originalPoint.X * cosTheta - originalPoint.Y * sinTheta),       //X
				(float)(originalPoint.X * sinTheta + originalPoint.Y * cosTheta));		//Y

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
			Point[] corners = GetSkewedCorners(b.BoardCorners, b.CurrentRotation, b.Center);
			e.Graphics.DrawImage(b.BoardImage, corners);

			if(PieceIsSelected)
			{
				for (int i = 0; i < 8; i++)
					for (int j = 0; j < 8; j++)
						if (vm.IsValidMove(Board, CurrentPlayer, lastSelectedY, lastSelectedX, i, j))
						{
							PointF p = ProjectVectorToPlane(RotateVector(new PointF((float)(15 * (j - 4) + 8), (float)(15 * (i - 4) + 5.5)), Board.CurrentRotation), Board.CosCameraAngle);

							int sizex = 32;
							int sizey = 32;

							PointF p2 = new PointF(p.X * Board.ScaleFactor + Board.Center.X - sizex/2-2, p.Y * Board.ScaleFactor + Board.Center.Y - sizey/2-1);	
							e.Graphics.DrawImage(Circle,new RectangleF(p2, new Size(sizex, sizey)));
						}
			}


		}

		void DrawChessPiece(PaintEventArgs e, ChessBoard b, ChessPiece cp)
		{
			e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			///asdasd
			PointF point = cp.RelativeCoords;

			float sizex = 65;
			float sizey = 65;
			
			float height = cp.Selected == false ? 4 : 6 + 2*(float)Math.Sin((sw.ElapsedMilliseconds - cp.TimeStartedFloating)/1000 * Math.PI/2 - Math.PI/2);
			RectangleF rect = new RectangleF
			(
				point.X * b.ScaleFactor - sizex / 2 + b.Center.X,
				point.Y * b.ScaleFactor - sizey / 2 + b.Center.Y - height * Board.ScaleFactor * (float)Math.Sin(Board.CameraAngle),
				sizex, sizey
			);

			if(cp.Selected == true)
			{
				PointF p = new PointF(point.X * b.ScaleFactor + b.Center.X, point.Y * b.ScaleFactor + b.Center.Y);
				int sizex2 = 64;
				int sizey2 = 64;

				ColorMatrix colormatrix = new ColorMatrix();
				float opacity = 2f * (float)Math.Sin((sw.ElapsedMilliseconds - cp.TimeStartedFloating) / 1000 * Math.PI / 2 - Math.PI / 2);
				colormatrix.Matrix33 = Math.Max(0, Math.Min(1, opacity));
				ImageAttributes attributes = new ImageAttributes();
				attributes.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				e.Graphics.DrawImage(Shadow, new Rectangle(Point.Round(p-new Size(sizex2/2, sizey2*3/4)), new Size(sizex2, sizey2)), 0, 0, Shadow.Width, Shadow.Height, GraphicsUnit.Pixel, attributes);
			}

			int imgIndex = PieceSpriteIndex[cp.SpriteSet];
			e.Graphics.DrawImage(PieceSprites[imgIndex][((int)cp.Type)], rect);

			//Pen p = new Pen(Brushes.Red, 5);
			//e.Graphics.DrawLine(p, b.Center, new PointF(point.X + b.Center.X, point.Y+b.Center.Y));
			//e.Graphics.FillRectangle(Brushes.Red, new RectangleF(new PointF(point.X + b.Center.X, point.Y + b.Center.Y), new SizeF(1, 1)));
			//e.Graphics.FillEllipse(Brushes.Red, point.X+b.Center.X - 2, point.Y+b.Center.Y-2, 4, 4);
		}

		void ChangeRotation(double theta)
		{
			Board.CurrentRotation = theta;
			for(int i = 0; i < Board.Pieces.Count; i++)
			{
				Board.Pieces[i].RelativeCoords = Board.Pieces[i].CalcRelativeCoords(Board.CurrentRotation);
			}
		}
		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			DrawChessBoard(e, Board);

			List<ChessPiece> ordered = Board.Pieces.OrderBy(o => o.RelativeCoords.Y).ToList();

			foreach ( ChessPiece cp in ordered)
			{
				if(cp.PositionOnBoard.Item1 != -1 && cp.PositionOnBoard.Item2 != -1)
				DrawChessPiece(e, Board, cp);
			}

		}

		private void pictureBox1_SizeChanged(object sender, EventArgs e)
		{
			Board.Center = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
		}

		int lastSelectedIndex = -1;
		int lastSelectedX = -1;
		int lastSelectedY = -1;
		VerifyMoves vm = new VerifyMoves();

		void UnselectPiece()
		{
			Board.Pieces[lastSelectedIndex].Selected = false;
			PieceIsSelected = false;
		}
		private void pictureBox1_Click(object sender, EventArgs e)
		{
			if (CanSelect == false)
				return;
			MouseEventArgs mouseEventArgs = (MouseEventArgs)e;

			PointF p = new PointF(
				(mouseEventArgs.Location.X - Board.Center.X) / Board.ScaleFactor,
				(mouseEventArgs.Location.Y - Board.Center.Y) / Board.ScaleFactor / (float)Board.CosCameraAngle
				);
			PointF p2 = RotateVector(p, -Board.CurrentRotation);
			PointF clickLocationOnBoard = new PointF(p2.X / 15 + 4, p2.Y / 15 + 4);

			int x = (int)clickLocationOnBoard.X;
			int y = (int)clickLocationOnBoard.Y;
			
			//verify square is in board
			if(x < 0 || x >= 8 || y < 0 || y >= 8)
			{
				if (lastSelectedIndex != -1)
				{
					UnselectPiece();
				}
				Console.WriteLine("Clicked outside of board!");
				return;
			}


			if (PieceIsSelected == true)
			{
				if (vm.IsValidMove(Board, CurrentPlayer, lastSelectedY, lastSelectedX, y, x))
				{
					Console.WriteLine("YES");
					/**
					Console.WriteLine("Currentplayer: " + CurrentPlayer);
					Console.WriteLine("owner1: " + Board.Pieces[lastSelectedIndex].Owner);
					if(Board.BoardState[y, x] != -1)
					Console.WriteLine("owner2: " + Board.Pieces[Board.BoardState[y, x]].Owner);
					**/

					if (Board.BoardState[y,x] != -1) 
					{
						Board.Pieces[Board.BoardState[y, x]].PositionOnBoard = new Tuple<int, int>(-1, -1);
					}

					Board.BoardState[y, x] = Board.BoardState[lastSelectedY, lastSelectedX];
					Board.BoardState[lastSelectedY, lastSelectedX] = -1;
					Board.Pieces[lastSelectedIndex].PositionOnBoard = new Tuple<int, int>(y, x);


					UnselectPiece();
					CurrentPlayer = CurrentPlayer == "Player1" ? "Player2" : "Player1";
					TimeStartBoardRotation = sw.ElapsedMilliseconds;
					AccumulatedAngle += Math.PI;
					CanSelect = false;
					return;
				}
				else
					Console.WriteLine("NO");
			}


			//Clicked square is occupied
			if (Board.BoardState[y, x] != -1)
			{
				Console.WriteLine("Clicked X:" + x + " Y:" + y + " Piece: " + Board.Pieces[Board.BoardState[y, x]].Type);
				
				if(x == lastSelectedX && y == lastSelectedY && PieceIsSelected)
				{
					UnselectPiece();
					return;
				}

				// Update selected
				if (lastSelectedIndex != -1)
					Board.Pieces[lastSelectedIndex].Selected = false;

				if(Board.Pieces[Board.BoardState[y, x]].Owner == CurrentPlayer)
				{
					Board.Pieces[Board.BoardState[y, x]].Selected = true;
					PieceIsSelected = true;
					lastSelectedX = x;
					lastSelectedY = y;
					Board.Pieces[Board.BoardState[y, x]].TimeStartedFloating = sw.ElapsedMilliseconds;
					lastSelectedIndex = Board.BoardState[y, x];
				}
			}
			else
			{
				Console.WriteLine("Clicked X:" + x + " Y:" + y + " Piece: None");
				if(PieceIsSelected)
					UnselectPiece();
			}

		}

		int seconds = 0;
		private void timer1_Tick(object sender, EventArgs e)
		{
			seconds++;
			label1.Text = seconds/60 + ":" + (seconds%60 < 10 ? "0" : "")+ seconds%60;
		}
	}
}
