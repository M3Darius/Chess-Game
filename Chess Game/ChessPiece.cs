using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Chess_Game.Form1;

namespace Chess_Game
{
	public class ChessPiece
	{
		public Tuple<int, int> PositionOnBoard;
		ChessBoard Board;
		public SpriteType Type;
		public string SpriteSet;
		public PointF RelativeCoords;
		public string Owner;
		public bool Moved;
		public bool Selected;
		public double TimeStartedFloating;

		public ChessPiece(int i, int j, SpriteType type, ChessBoard board, string owner, string spriteSet)
		{
			PositionOnBoard = new Tuple<int, int>(i, j);
			Type = type;
			Board = board;
			RelativeCoords = CalcRelativeCoords(Board.CurrentRotation);
			Owner = owner;
			Moved = false;
			SpriteSet = spriteSet;
			TimeStartedFloating = 0;
		}
		public PointF CalcRelativeCoords(double theta) ///relative to center of the board
		{
			int i = PositionOnBoard.Item1, j = PositionOnBoard.Item2;

			return ProjectVectorToPlane(RotateVector(new PointF((float)(15 * (j - 4) + 8), (float)(15 * (i - 4) + 5.5)), theta), Board.CosCameraAngle);

		}

	}

	public class VerifyMoves
	{
		/**
		public bool VerifyPawnMove(ChessBoard board, int player, int i, int j, int targeti, int targetj)
		{
			if (player == 1)
			{
				
				if(targeti - i == 1 && j == targetj)
			}
			else
			if (player == 2)
			{

			}
			return true;
		}

		public bool VerifyKingMove(int player, int i, int j, int targeti, int targetj)
		{

		}

		public bool VerifyBishopMove(ChessBoard board, string player, int i, int j, int targeti, int targetj)
		{

		}
		**/

		public bool IsValidMove(ChessBoard board, string CurrentPlayer, int i, int j, int targeti, int targetj)
		{
			if (i == targeti && j == targetj)
				return false;

			if (board.Pieces[board.BoardState[i,j]].Owner != CurrentPlayer)
				return false;

			if (board.BoardState[targeti, targetj] != -1 && board.Pieces[board.BoardState[targeti, targetj]].Owner == CurrentPlayer)
				return false;

			switch (board.Pieces[board.BoardState[i, j]].Type)
			{
				case SpriteType.Pawn:
					return VerifyPawnMove(board, CurrentPlayer, i, j, targeti, targetj);
				case SpriteType.Rook:
					return VerifyRookMove(board, i, j, targeti, targetj);
				case SpriteType.Knight:
					return VerifyKnightMove(board, i, j, targeti, targetj);
				case SpriteType.Bishop:
					return VerifyBishopMove(board, i, j, targeti, targetj);
				case SpriteType.Queen:
					return VerifyRookMove(board, i, j, targeti, targetj) || VerifyBishopMove(board, i, j, targeti, targetj);
				case SpriteType.King:
					return VerifyKingMove(board, i, j, targeti, targetj);
				case SpriteType.None:
					break;
				default:
					break;
			}
			return false;
		}

		public bool VerifyKnightMove(ChessBoard board, int i, int j, int targeti, int targetj)
		{
			int disti = Math.Abs(targeti - i);
			int distj = Math.Abs(targetj - j);
			if (disti <= 2 && distj <= 2 && disti + distj == 3)
				return true;

			return false;
		}
		public bool VerifyPawnMove(ChessBoard board, string player, int i, int j, int targeti, int targetj)
		{
			if (player == "Player2") // y++
			{
				if (i == targeti + 1 && j == targetj)
					return true;
				if (i == 6 && targeti == 4 && j == targetj)// && board.BoardState[i + 1, j] == -1)
					return true;
				if (i == targeti + 1 && Math.Abs(j - targetj) == 1 && board.BoardState[targeti, targetj] != -1)
					return true;
			}
			if (player == "Player1") // y++
			{
				if (i == targeti - 1 && j == targetj)
					return true;
				if (i == 1 && targeti == 3 && j == targetj) // && board.BoardState[i - 1, j] == -1)
					return true;
				if (i == targeti - 1 && Math.Abs(j - targetj) == 1 && board.BoardState[targeti, targetj] != -1)
					return true;
			}


			return false;
		}
		public bool VerifyKingMove(ChessBoard board, int i, int j, int targeti, int targetj)
		{
			if(Math.Abs(targeti - i) > 1 || Math.Abs(targetj - j) > 1)
				return false;

			return true;
		}
		public bool VerifyBishopMove(ChessBoard board, int i, int j, int targeti, int targetj)
		{
			if(targeti - targetj != i - j && targeti + targetj != i + j)
				return false;

			if (targeti - targetj == i - j)
			{
				int min = Math.Min(i, targeti);
				int max = Math.Max(i, targeti);
				int c = i - j;
				for(int k = min+1; k < max; k++)
				{
					if (board.BoardState[k, k-c] != -1)
						return false;
				}
			}
			if (targeti + targetj == i + j)
			{
				int min = Math.Min(i, targeti);
				int max = Math.Max(i, targeti);
				int c = i + j - 7;
				for (int k = min + 1; k < max; k++)
				{
					if (board.BoardState[k, 7+c-k] != -1)
						return false;
				}
			}


			return true;
		}
		public bool VerifyRookMove(ChessBoard board, int i, int j, int targeti, int targetj)
		{
			if (i != targeti && j != targetj)
				return false;

			if(i == targeti)
			{
				int min = Math.Min(j, targetj);
				int max = Math.Max(j, targetj);
				for (int k = min+1; k < max; k++)
				{
					if (board.BoardState[i, k] != -1)
						return false;
				}
			}

			if(j == targetj)
			{
				int min = Math.Min(i, targeti);
				int max = Math.Max(i, targeti);
				for (int k = min + 1; k < max; k++)
				{
					if (board.BoardState[k, j] != -1)
						return false;
				}
			}

			return true;
		}
	}
}
