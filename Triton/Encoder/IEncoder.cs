namespace Triton.Encoder;

public interface IEncoder {
	public void Write(Stream stream, ImageCollection frames);
	public ImageCollection Read(Stream stream);
}
