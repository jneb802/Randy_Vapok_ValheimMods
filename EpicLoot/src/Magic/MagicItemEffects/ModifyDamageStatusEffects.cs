namespace EpicLoot.MagicItemEffects;

public class ModifyDamageStatusEffects
{
    public static bool HasStatusEffect(Player player, string StatusEffect)
    {
        if (player == null)
        {
            return false;
        }
        
        if (player.GetSEMan() == null)
        {
            return false;
        }
        
        if (player.GetSEMan().HaveStatusEffect(StatusEffect.GetStableHashCode()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}