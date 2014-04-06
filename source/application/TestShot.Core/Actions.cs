namespace TestShot
{
	using System;
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public enum Action
	{
		Unknown,
		Screenshot,
		Click,
		Keypress,
		Pause,
		Scroll,
		Note
	}
}