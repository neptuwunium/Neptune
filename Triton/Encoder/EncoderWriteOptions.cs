namespace Triton.Encoder;

public record EncoderWriteOptions {
	public static EncoderWriteOptions Default { get; } = new();

	public bool Compress { get; init; } = true;
	public bool AssociateAlpha { get; init; }
}
