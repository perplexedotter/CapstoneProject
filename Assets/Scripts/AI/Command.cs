using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public enum CommandType { Move, Action };

    public Tile target;
    public CommandType commandType;
    public Action action;

    public Command(Tile target, CommandType commandType, Action action)
    {
        this.target = target;
        this.commandType = commandType;
        this.action = action;
    }
}
