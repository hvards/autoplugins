using AutoContracts;

namespace WindowKeys;

public class WindowPickerCommand : ICommand
{
	private OverlayHost _host = null!;

	public string Name => "WindowPicker";
	public string Description => "Show overlay on each window and pick one with keyboard.";
	public Guid Id { get; } = Guid.Parse("7f3a9b2c-4d5e-6f71-8a9b-0c1d2e3f4a5b");
	public Type ReturnType { get; } = typeof(string);
	public bool RequiresSta => true;
	public List<PluginArgument> ExpectedArguments { get; } = [];

	public void Init() => _host = new OverlayHost();

	public object? Execute(object?[] args) => _host.RunPicker();
}