using System;

public class Version : IComparable
{
	private int major;
	private int minor;
	private int revision;
	private int build;

	public static readonly char Separator = '.';

	public Version(int major, int minor, int revision, int build)
	{
		this.major = major;
		this.minor = minor;
		this.revision = revision;
		this.build = build;
	}
	


	public static Version Parse(string versionText)
	{
		int major = 0;
		int minor = 0;
		int revision = 0;
		int build = 0;
		var v = versionText.Split(Separator);

		for (int i = 0; i < v.Length; i++)
		{
			int numericVersion = 0;
			if (int.TryParse(v[i], out numericVersion))
			{
				switch (i)
				{
					case 0:
						major = numericVersion;
						break;
					case 1:
						minor = numericVersion;
						break;
					case 2:
						revision = numericVersion;
						break;
					case 3:
						build = numericVersion;
						break;
					default:
						break;
				}
			}
		}

		return new Version(major,minor,revision,build); ;
	}

	public int CompareTo(object obj)
	{
		var v = obj as Version;
		var majorComparison = major.CompareTo(v?.major);
		if (majorComparison == 0)
		{
			var minorComparison = minor.CompareTo(v?.minor);
			if (minorComparison == 0)
			{
				var revisionComparison = revision.CompareTo(v?.revision);
				if (revisionComparison == 0)
					return build.CompareTo(v?.build);
				else
					return revisionComparison;
			}
			else
				return minorComparison;
		}
		else
			return majorComparison;
	}
}

