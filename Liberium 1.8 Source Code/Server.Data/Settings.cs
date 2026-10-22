namespace Server.Data;

public class Settings
{
	public string[] Ports;

	public bool Start;

	public int second = 35;

	public string WebHook;

	public bool WebHookNewConnect;

	public bool WebHookConnect;

	public bool AutoStealer;

	public int Style;

	public int Theme = 0; // 0 = Light, 1 = Dark

	public string linkMiner = "http://%IP%/ack";
}

