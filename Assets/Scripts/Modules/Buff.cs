
enum EffectedStat { Health, Speed, Shields}

public class Buff
{
    readonly EffectedStat EffectedStat;
    readonly int Power;

    Buff(EffectedStat EffectedStat, int Power)
    {
        this.EffectedStat = EffectedStat;
        this.Power = Power;
    }

}