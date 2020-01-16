using UnityEngine;
using System.Collections;


//CardPlay->hasEffect->CardType->Active->CardEvent->ActiveCommand
public class CreatureDamagePER : CreatureEffect
{
    public CreatureDamagePER(Player owner, CreatureLogic creature, int specialAmount): base(owner, creature, specialAmount)
    {}

    // BATTLECRY
    public override void WhenACreatureIsPlayed()
    {
        new DealDamageCommand(owner.otherPlayer.PlayerID, specialAmount, owner.otherPlayer.Health - specialAmount).AddToQueue();
        owner.otherPlayer.Health -= specialAmount;
    }
}
