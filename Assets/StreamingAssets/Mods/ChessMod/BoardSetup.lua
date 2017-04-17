-- Copyright (c) 2017, Timothy Ned Atton.
-- All rights reserved.
-- nedmakesgames@gmail.com
-- This code was written while streaming on twitch.tv/nedmakesgames
--
-- This file is part of Moddable Chess.
--
-- Moddable Chess is free software: you can redistribute it and/or modify
-- it under the terms of the GNU General Public License as published by
-- the Free Software Foundation, either version 3 of the License, or
-- (at your option) any later version.
--
-- Moddable Chess is distributed in the hope that it will be useful,
-- but WITHOUT ANY WARRANTY; without even the implied warranty of
-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
-- GNU General Public License for more details.
--
-- You should have received a copy of the GNU General Public License
-- along with Moddable Chess.  If not, see <http://www.gnu.org/licenses/>.

return function(board)
	board.addPiece("Rook", "First", 1, 1)
	board.addPiece("Knight", "First", 2, 1)
	board.addPiece("Bishop", "First", 3, 1)
	board.addPiece("Queen", "First", 4, 1)
	board.addPiece("King", "First", 5, 1)
	board.addPiece("Bishop", "First", 6, 1)
	board.addPiece("Knight", "First", 7, 1)
	board.addPiece("Rook", "First", 8, 1)
	
	board.addPiece("Rook", "Second", 1, 8)
	board.addPiece("Knight", "Second", 2, 8)
	board.addPiece("Bishop", "Second", 3, 8)
	board.addPiece("Queen", "Second", 4, 8)
	board.addPiece("King", "Second", 5, 8)
	board.addPiece("Bishop", "Second", 6, 8)
	board.addPiece("Knight", "Second", 7, 8)
	board.addPiece("Rook", "Second", 8, 8)
	
	for i = 1, 8 do
		board.addPiece("Pawn", "First", i, 2)
		board.addPiece("Pawn", "Second", i, 7)
	end
end