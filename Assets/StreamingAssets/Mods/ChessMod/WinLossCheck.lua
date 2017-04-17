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
	local firstHasKing = false
	local secondHasKing = false
	for pieceIndex = 1, board.numPieces do
		local piece = board.getPiece(pieceIndex)
		if not piece.isCaptured and piece.pieceName == "King" then
			if piece.owner == helper.firstPlayer then
				firstHasKing = true
			else
				secondHasKing = true
			end
		end
	end
	if not firstHasKing and not secondHasKing then
		return winCheckHelper.getTieState()
	elseif not firstHasKing then
		return winCheckHelper.getWinState(helper.secondPlayer)
	elseif not secondHasKing then
		return winCheckHelper.getWinState(helper.firstPlayer)
	else
		return winCheckHelper.getUndecidedState()
	end
end