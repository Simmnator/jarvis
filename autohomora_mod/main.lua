  -- Author: VTLI#9513 (Discord: Hogwarts Legacy Modding)

-- ##############################
-- Local Vars
-- ##############################

local Player = nil
local Lock = nil

-- ##############################
-- Hooks
-- ##############################


RegisterHook("/Script/Phoenix.LockableComponent:CanPlayerUseAlohomoraOnLock", function(Context)
    if Lock then
    Lock.FadeBlackDuration = 0
    Lock.PuzzleComplete = true
    Lock:Exit()
    Lock = nil
    else

    end
end)

RegisterHook("/Script/Engine.PlayerController:ClientRestart", function(Context, NewPawn)

    Player = Context:get().Pawn

    RegisterHook("/Game/RiggedObjects/Environments/AlohomoraLock/BP_AlohomoraLock.BP_AlohomoraLock_C:ReceiveBeginPlay", function(Context)
        Lock = Context:get()
    end)
end)