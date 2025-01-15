using Ink.Entities.Damage;
using Ink.Nbt;
using Ink.Nbt.Serialization;

namespace Ink.Util.Converter.Nbt;

public sealed class DamageTypeNbtConverter : NbtConverter<DamageType>
{
    public static readonly DamageTypeNbtConverter Shared = new();

    private DamageTypeNbtConverter()
    {
    }

    public override DamageType Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, DamageType value)
    {
        writer.WriteCompoundStart();
        
        if(!string.IsNullOrEmpty(value.MessageId))
            writer.WriteString("message_id", value.MessageId);

        writer.WriteFloat("exhaustion", value.Exhaustion);
        writer.WriteString("scaling", value.Scaling.ToStringFast());

        if(value.Effects != DamageEffect.Hurt)
            writer.WriteString("effects", value.Effects.ToStringFast());

        if(value.DeathMessageType != DeathMessageType.Default)
            writer.WriteString("death_message_type", value.DeathMessageType.ToStringFast());

        writer.WriteCompoundEnd();
    }
}
