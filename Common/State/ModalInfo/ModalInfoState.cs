namespace Common.State.ModalInfo;

using Fluxor;

[FeatureState]
public class ModalInfoState
{
	public string Text { get; set; } = string.Empty;
	public bool IsError { get; set; }

	public ModalInfoState(string text, bool isError = false)
	{
		Text = text;
		IsError = isError;
	}

	public ModalInfoState()
	{
	}
}
