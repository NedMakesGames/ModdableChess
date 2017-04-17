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

local moveToStatus = function(movingPiece, board, toX, toY)
	local pieceAt = board.pieceAtPosition(toX, toY)
	if pieceAt > 0 then
		if board.getPiece(pieceAt).owner == movingPiece.owner then
			return "blocked"
		else
			return "enemy"
		end
	else
		return "open"
	end
end

local lookAhead = function(possibleActions, movingPiece, board, incrX, incrY)
	local atX = movingPiece.positionX + incrX
	local atY = movingPiece.positionY + incrY
	while board.isValidPosition(atX, atY) do
		local moveStatus = moveToStatus(movingPiece, board, atX, atY)
		if moveStatus == "blocked" then
			break
		elseif moveStatus == "enemy" then
			table.insert(possibleActions, {actionHelper.newCaptureAction(atX, atY), actionHelper.newMoveAction(movingPiece, atX, atY)})
			break;
		else
			table.insert(possibleActions, {actionHelper.newMoveAction(movingPiece, atX, atY)})
		end
		atX = atX + incrX
		atY = atY + incrY
	end
end

local lookAtPosition = function(possibleActions, movingPiece, board, offX, offY)
	local atX = movingPiece.positionX + offX
	local atY = movingPiece.positionY + offY
	if board.isValidPosition(atX, atY) then
		local moveStatus = moveToStatus(movingPiece, board, atX, atY)
		if moveStatus == "enemy" then
			table.insert(possibleActions, {actionHelper.newCaptureAction(atX, atY), actionHelper.newMoveAction(movingPiece, atX, atY)})
		elseif moveStatus == "open" then
			table.insert(possibleActions, {actionHelper.newMoveAction(movingPiece, atX, atY)})
		end
	end
end

rookMovement = function(state)
	local movingPiece = state.board.getPiece(state.movingPiece)
	local possibleActions = {}
	lookAhead(possibleActions, movingPiece, state.board, 1, 0)
	lookAhead(possibleActions, movingPiece, state.board, -1, 0)
	lookAhead(possibleActions, movingPiece, state.board, 0, 1)
	lookAhead(possibleActions, movingPiece, state.board, 0, -1)
	return possibleActions
end

bishopMovement = function(state)
	local movingPiece = state.board.getPiece(state.movingPiece)
	local possibleActions = {}
	lookAhead(possibleActions, movingPiece, state.board, 1, 1)
	lookAhead(possibleActions, movingPiece, state.board, -1, -1)
	lookAhead(possibleActions, movingPiece, state.board, -1, 1)
	lookAhead(possibleActions, movingPiece, state.board, 1, -1)
	return possibleActions
end
	
queenMovement = function(state)
	local movingPiece = state.board.getPiece(state.movingPiece)
	local possibleActions = {}
	lookAhead(possibleActions, movingPiece, state.board, 1, 0)
	lookAhead(possibleActions, movingPiece, state.board, -1, 0)
	lookAhead(possibleActions, movingPiece, state.board, 0, 1)
	lookAhead(possibleActions, movingPiece, state.board, 0, -1)
	lookAhead(possibleActions, movingPiece, state.board, 1, 1)
	lookAhead(possibleActions, movingPiece, state.board, -1, -1)
	lookAhead(possibleActions, movingPiece, state.board, -1, 1)
	lookAhead(possibleActions, movingPiece, state.board, 1, -1)
	return possibleActions
end

knightMovement = function(state)
	local movingPiece = state.board.getPiece(state.movingPiece)
	local possibleActions = {}
	lookAtPosition(possibleActions, movingPiece, state.board, 2, 1)
	lookAtPosition(possibleActions, movingPiece, state.board, 2, -1)
	lookAtPosition(possibleActions, movingPiece, state.board, -2, 1)
	lookAtPosition(possibleActions, movingPiece, state.board, -2, -1)
	lookAtPosition(possibleActions, movingPiece, state.board, 1, 2)
	lookAtPosition(possibleActions, movingPiece, state.board, 1, -2)
	lookAtPosition(possibleActions, movingPiece, state.board, -1, 2)
	lookAtPosition(possibleActions, movingPiece, state.board, -1, -2)
	return possibleActions
end

kingMovement = function(state)
	local movingPiece = state.board.getPiece(state.movingPiece)
	local possibleActions = {}
	lookAtPosition(possibleActions, movingPiece, state.board, 0, 1)
	lookAtPosition(possibleActions, movingPiece, state.board, 0, -1)
	lookAtPosition(possibleActions, movingPiece, state.board, 1, 0)
	lookAtPosition(possibleActions, movingPiece, state.board, -1, 0)
	lookAtPosition(possibleActions, movingPiece, state.board, 1, 1)
	lookAtPosition(possibleActions, movingPiece, state.board, 1, -1)
	lookAtPosition(possibleActions, movingPiece, state.board, -1, 1)
	lookAtPosition(possibleActions, movingPiece, state.board, -1, -1)
	return possibleActions
end

local addPawnAction = function(possibleActions, movingPiece, x, y, capturing, promoting, promotionPiece)
	local actionList = {}
	if capturing then
		table.insert(actionList, actionHelper.newCaptureAction(x, y))
	end
	table.insert(actionList, actionHelper.newMoveAction(movingPiece, x, y))
	if promoting then
		table.insert(actionList, actionHelper.newPromoteAction(x, y, promotionPiece))
	end
	table.insert(possibleActions, actionList)
end

local pawnCheck = function(possibleActions, movingPiece, board, xOff, yOff, checkFunc, capturing, promoteY) 
	local px = movingPiece.positionX + xOff
	local py = movingPiece.positionY + yOff
	if board.isValidPosition(px, py) then
		local pieceAtIndex = board.pieceAtPosition(px, py)
		local pieceAt = pieceAtIndex > 0 and board.getPiece(pieceAtIndex) or nil
		if checkFunc(movingPiece, pieceAt) then
			--addPawnAction(possibleActions, movingPiece, px, py, capturing, py == promoteY)
			if py == promoteY then
				addPawnAction(possibleActions, movingPiece, px, py, capturing, true, "Queen")
				addPawnAction(possibleActions, movingPiece, px, py, capturing, true, "Knight")
				addPawnAction(possibleActions, movingPiece, px, py, capturing, true, "Rook")
				addPawnAction(possibleActions, movingPiece, px, py, capturing, true, "Bishop")
			else
				addPawnAction(possibleActions, movingPiece, px, py, capturing, false)
			end
			return true
		end
	end
	return false
end

local pawnCheckOnlyOpen = function(movingPiece, pieceAt)
	return pieceAt == nil
end

local pawnCheckOnlyEnemy = function(movingPiece, pieceAt)
	return pieceAt ~= nil and pieceAt.owner ~= movingPiece.owner
end

pawnMovement = function(state)
	local possibleActions = {}
	local movingPiece = state.board.getPiece(state.movingPiece)
	local yDir
	local promoteY
	if movingPiece.owner == helper.firstPlayer then
		yDir = 1
		promoteY = 8
	else
		yDir = -1
		promoteY = 1
	end
	
	local aheadOpen = pawnCheck(possibleActions, movingPiece, state.board, 0, yDir, pawnCheckOnlyOpen, false, promoteY)
	pawnCheck(possibleActions, movingPiece, state.board, 1, yDir, pawnCheckOnlyEnemy, true, promoteY)
	pawnCheck(possibleActions, movingPiece, state.board, -1, yDir, pawnCheckOnlyEnemy, true, promoteY)
	
	if aheadOpen and movingPiece.numberOfMoves == 0 then
		pawnCheck(possibleActions, movingPiece, state.board, 0, yDir * 2, pawnCheckOnlyOpen, false, promoteY)
	end
	
	return possibleActions
end
