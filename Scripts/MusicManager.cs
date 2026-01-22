using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MusicManager : Node2D
{
	CustomSignals _CustomSignals;
	[Export] NodePath Music1Path;
	[Export] NodePath Music2Path;
	AudioStreamPlayer _Music1;
	AudioStreamPlayer _Music2;
	bool CurrentMusic;

	//Dictionary<string, AudioStream> MusicDictionary = new Dictionary<string, AudioStream>();
	[Export] Dictionary<string, AudioStream> MusicDictionary = new Dictionary<string, AudioStream>();
	public override void _Ready()
	{
		_Music1 = GetNode<AudioStreamPlayer>(Music1Path);
		_Music2 = GetNode<AudioStreamPlayer>(Music2Path);
		_CustomSignals = GetNode<CustomSignals>("/root/CustomSignals");
		_CustomSignals.Connect("PlayMusicDelegat", this, nameof(PlayMusic));

		// MusicDictionary.Clear();
		// MusicDictionary.Add("Sound1", ResourceLoader.Load<AudioStream>("res://Sound/digital-racer.mp3"));
		// MusicDictionary.Add("Sound2", ResourceLoader.Load<AudioStream>("res://Sound/synthwave-spectrum.mp3"));
		// MusicDictionary.Add("Sound3", ResourceLoader.Load<AudioStream>("res://Sound/neon-dreams.mp3"));
		// MusicDictionary.Add("Sound4", ResourceLoader.Load<AudioStream>("res://Sound/synthwave-dreams.mp3"));

		_Music1.Stream = MusicDictionary["Intro"];
	}

	public void PlayMusic(string musicName)
	{
		MusicVolumeFade(MusicDictionary[musicName]);
	}
	public async void MusicVolumeFade(AudioStream music)
	{
		CurrentMusic = !CurrentMusic;
		_Music1.Play();
		_Music2.Play();

		if (CurrentMusic)
		{
			_Music2.Stream = music;
			//_Music1.Play();
			_Music2.Play();
			
			while (_Music2.VolumeDb < 0)
			{
				_Music1.VolumeDb -= 0.9f;
				_Music2.VolumeDb += 0.9f;
				await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
			}
			_Music1.Stop();
		}
		else
		{
			// aktuální skladba je v music2
			_Music1.Stream = music;
			_Music1.Play();
			//_Music2.Play();

			while (_Music1.VolumeDb < 0)
			{
				_Music1.VolumeDb += 0.9f;
				_Music2.VolumeDb -= 0.9f;
				await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
			}
			_Music2.Stop();
		}
	}
}
