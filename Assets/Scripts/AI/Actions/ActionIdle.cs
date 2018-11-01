﻿using BTree;

public class ActionIdle :BTAction  {

    public ActionIdle() : base() { }
    public ActionIdle(BTNode _parent)
            : base(_parent) { }

    protected override void OnEnter(BTInput _input)
    {
        base.OnEnter(_input);
    }

    protected override BTResult OnExecute(ref BTInput _input)
    {
        PlayerInputData input = _input as PlayerInputData;
        if (input == null || input.player == null)
        {
            
        }

        PlayerEntity player = input.player;

        player.PlayAnimation( PlayerAnimationType.idle, UnityEngine.WrapMode.Loop);

        return BTResult.Success;
    }
}
