using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct Item {
    public byte[] back;
}

[System.Serializable]
public class SyncListItem : SyncList<Item> { }
public class CameraDevice : NetworkBehaviour {
    GameController gameController;
    public NetworkManager manager;
    public GameObject teste;
    public WebCamTexture backCam;
    public Item item = new Item ();
    public readonly SyncListItem inventory = new SyncListItem ();

    [SyncVar]
    public int width;
    [SyncVar]
    public int height;

    void Start () {
        QualitySettings.masterTextureLimit = 1;
        if (!gameController) {
            gameController = GameObject.FindObjectOfType<GameController> ();
        }
        if (!manager) {
            manager = GetComponent<NetworkManager> ();
        }
        StartCoroutine (startCamera ());
    }

    IEnumerator startCamera () {
        if (SceneManager.GetActiveScene ().name == "AquariumSceneClient") {
            yield return Application.RequestUserAuthorization (UserAuthorization.WebCam);
            if (Application.HasUserAuthorization (UserAuthorization.WebCam)) {
                WebCamDevice[] devices = WebCamTexture.devices;
                WebCamDevice device = new WebCamDevice();
                for (int i = 0; i < devices.Length; i++){
                    Debug.Log(devices[i].name);
                    if(devices[i].isFrontFacing){
                        device = devices[i];
                    }
                }
                if (backCam == null) {
                    backCam = new WebCamTexture(device.name);
                }
                GetComponent<Renderer> ().material.mainTexture = backCam;

                if (!backCam.isPlaying) {
                    backCam.requestedWidth = 10;
                    backCam.requestedHeight = 10;
                    backCam.requestedFPS = 1;
                    backCam.Play ();
                    width = backCam.width;
                    height = backCam.height;
                }
            }
        }
    }

    void Update () {
        if (SceneManager.GetActiveScene ().name == "AquariumSceneClient") {
            Texture2D photo = new Texture2D (width, height, TextureFormat.RGB24, false);
            if (backCam.isPlaying)
                photo.SetPixels (backCam.GetPixels ());
            photo.Apply ();
            TextureScale.Bilinear (photo, width / 4, height / 4);
            byte[] t = photo.EncodeToJPG ();
            item.back = t;
            if (t.LongCount () < 10000) {
                inventory.Clear ();
                inventory.Add (item);
            }
        } else {
            if (inventory.Count > 0) {
                Texture2D photo = new Texture2D (width / 4, height / 4, TextureFormat.RGB24, false);
                byte[] t = inventory[0].back;
                photo.LoadImage (t);
                GetComponent<Renderer> ().material.mainTexture = photo;
            }
        }
    }

    public byte[] CompressCustom (byte[] buffer) {
        MemoryStream ms = new MemoryStream ();
        GZipStream zip = new GZipStream (ms, CompressionMode.Compress, true);
        zip.Write (buffer, 0, buffer.Length);
        zip.Close ();
        ms.Position = 0;

        MemoryStream outStream = new MemoryStream ();

        byte[] compressed = new byte[ms.Length];
        ms.Read (compressed, 0, compressed.Length);

        byte[] gzBuffer = new byte[compressed.Length + 4];
        Buffer.BlockCopy (compressed, 0, gzBuffer, 4, compressed.Length);
        Buffer.BlockCopy (BitConverter.GetBytes (buffer.Length), 0, gzBuffer, 0, 4);
        return gzBuffer;
    }

    public byte[] DecompressCustom (byte[] data) {
        MemoryStream input = new MemoryStream (data);
        MemoryStream output = new MemoryStream ();
        using (DeflateStream dstream = new DeflateStream (input, CompressionMode.Decompress)) {
            dstream.CopyTo (output);
        }
        return output.ToArray ();
    }
    /*[ClientCallback]
    void UpdateClient () {
        Debug.Log ("passou aqui");
        Texture2D photo = new Texture2D (640, 360);
        photo.LoadImage (inventory[0].back);
        GetComponent<Renderer> ().material.mainTexture = photo;
    }*/
}