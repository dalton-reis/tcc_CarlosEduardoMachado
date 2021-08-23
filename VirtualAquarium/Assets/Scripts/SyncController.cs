using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public static class WebTextureReaderWriter {
    public static void WriteB (this NetworkWriter writer, byte[] foto) {
        writer.WriteBytesAndSize(foto);
    }

    public static byte[] ReadWebTexture (this NetworkReader reader) {
        return reader.ReadBytesAndSize();
    }
}