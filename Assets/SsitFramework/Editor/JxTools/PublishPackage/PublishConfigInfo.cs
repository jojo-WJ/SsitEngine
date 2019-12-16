using UnityEngine;

[System.Serializable]
public class PublishConfigInfo : ScriptableObject
{
    public PublishConfig mConfig;

    public void Initialize()
    {

    }

}




[System.Serializable]
public class PublishConfig
{
    public GameEngineConfig baseConfig;

    public PcConfig pcConfig;

    public IosConfig iosConfig;

    public AndriodConfig andriodConfig;

}
[System.Serializable]

public class GameEngineConfig
{
    public string CompanyName;
    public string ProductName;
}
[System.Serializable]

public class PcConfig
{
    public string OutPath;
}
[System.Serializable]
public class AndriodConfig
{
    public string OutPath;

    public string ScriptingDefine;

    public string ApplicationIdentifier;

    public string BundleVersion;

    public int BundleVersionCode;

    public string KeystoreName;

    public string KeystorePass;
}

[System.Serializable]
public class IosConfig
{
    public string OutPath;
}