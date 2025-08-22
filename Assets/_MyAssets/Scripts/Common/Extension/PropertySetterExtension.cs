namespace MyScripts.Common.Extension;

internal static class PropertySetterExtension
{
    internal static void SetPosX(this Transform tf, float x)
    {
        Vector3 pos = tf.position;
        pos.x = x;
        tf.position = pos;
    }

    internal static void SetPosY(this Transform tf, float y)
    {
        Vector3 pos = tf.position;
        pos.y = y;
        tf.position = pos;
    }

    internal static void SetPosZ(this Transform tf, float z)
    {
        Vector3 pos = tf.position;
        pos.z = z;
        tf.position = pos;
    }

    internal static void SetPosXY(this Transform tf, float x, float y)
    {
        Vector3 pos = tf.position;
        pos.x = x;
        pos.y = y;
        tf.position = pos;
    }

    internal static void SetPosXZ(this Transform tf, float x, float z)
    {
        Vector3 pos = tf.position;
        pos.x = x;
        pos.z = z;
        tf.position = pos;
    }

    internal static void SetPosYZ(this Transform tf, float y, float z)
    {
        Vector3 pos = tf.position;
        pos.y = y;
        pos.z = z;
        tf.position = pos;
    }

    internal static void SetPosXYZ(this Transform tf, float x, float y, float z)
    {
        Vector3 pos = tf.position;
        pos.x = x;
        pos.y = y;
        pos.z = z;
        tf.position = pos;
    }

    internal static void SetLocalPosX(this Transform tf, float x)
    {
        Vector3 pos = tf.localPosition;
        pos.x = x;
        tf.localPosition = pos;
    }

    internal static void SetLocalPosY(this Transform tf, float y)
    {
        Vector3 pos = tf.localPosition;
        pos.y = y;
        tf.localPosition = pos;
    }

    internal static void SetLocalPosZ(this Transform tf, float z)
    {
        Vector3 pos = tf.localPosition;
        pos.z = z;
        tf.localPosition = pos;
    }

    internal static void SetLocalPosXY(this Transform tf, float x, float y)
    {
        Vector3 pos = tf.localPosition;
        pos.x = x;
        pos.y = y;
        tf.localPosition = pos;
    }

    internal static void SetLocalPosXZ(this Transform tf, float x, float z)
    {
        Vector3 pos = tf.localPosition;
        pos.x = x;
        pos.z = z;
        tf.localPosition = pos;
    }

    internal static void SetLocalPosYZ(this Transform tf, float y, float z)
    {
        Vector3 pos = tf.localPosition;
        pos.y = y;
        pos.z = z;
        tf.localPosition = pos;
    }

    internal static void SetLocalPosXYZ(this Transform tf, float x, float y, float z)
    {
        Vector3 pos = tf.localPosition;
        pos.x = x;
        pos.y = y;
        pos.z = z;
        tf.localPosition = pos;
    }

    internal static void SetRotX(this Transform tf, float x)
    {
        Vector3 rot = tf.eulerAngles;
        rot.x = x;
        tf.eulerAngles = rot;
    }

    internal static void SetRotY(this Transform tf, float y)
    {
        Vector3 rot = tf.eulerAngles;
        rot.y = y;
        tf.eulerAngles = rot;
    }

    internal static void SetRotZ(this Transform tf, float z)
    {
        Vector3 rot = tf.eulerAngles;
        rot.z = z;
        tf.eulerAngles = rot;
    }

    internal static void SetRotXY(this Transform tf, float x, float y)
    {
        Vector3 rot = tf.eulerAngles;
        rot.x = x;
        rot.y = y;
        tf.eulerAngles = rot;
    }

    internal static void SetRotXZ(this Transform tf, float x, float z)
    {
        Vector3 rot = tf.eulerAngles;
        rot.x = x;
        rot.z = z;
        tf.eulerAngles = rot;
    }

    internal static void SetRotYZ(this Transform tf, float y, float z)
    {
        Vector3 rot = tf.eulerAngles;
        rot.y = y;
        rot.z = z;
        tf.eulerAngles = rot;
    }

    internal static void SetRotXYZ(this Transform tf, float x, float y, float z)
    {
        Vector3 rot = tf.eulerAngles;
        rot.x = x;
        rot.y = y;
        rot.z = z;
        tf.eulerAngles = rot;
    }

    internal static void SetLocalRotX(this Transform tf, float x)
    {
        Vector3 rot = tf.localEulerAngles;
        rot.x = x;
        tf.localEulerAngles = rot;
    }

    internal static void SetLocalRotY(this Transform tf, float y)
    {
        Vector3 rot = tf.localEulerAngles;
        rot.y = y;
        tf.localEulerAngles = rot;
    }

    internal static void SetLocalRotZ(this Transform tf, float z)
    {
        Vector3 rot = tf.localEulerAngles;
        rot.z = z;
        tf.localEulerAngles = rot;
    }

    internal static void SetLocalRotXY(this Transform tf, float x, float y)
    {
        Vector3 rot = tf.localEulerAngles;
        rot.x = x;
        rot.y = y;
        tf.localEulerAngles = rot;
    }

    internal static void SetLocalRotXZ(this Transform tf, float x, float z)
    {
        Vector3 rot = tf.localEulerAngles;
        rot.x = x;
        rot.z = z;
        tf.localEulerAngles = rot;
    }

    internal static void SetLocalRotYZ(this Transform tf, float y, float z)
    {
        Vector3 rot = tf.localEulerAngles;
        rot.y = y;
        rot.z = z;
        tf.localEulerAngles = rot;
    }

    internal static void SetLocalRotXYZ(this Transform tf, float x, float y, float z)
    {
        Vector3 rot = tf.localEulerAngles;
        rot.x = x;
        rot.y = y;
        rot.z = z;
        tf.localEulerAngles = rot;
    }

    internal static void SetScaleX(this Transform tf, float x)
    {
        Vector3 scale = tf.localScale;
        scale.x = x;
        tf.localScale = scale;
    }

    internal static void SetScaleY(this Transform tf, float y)
    {
        Vector3 scale = tf.localScale;
        scale.y = y;
        tf.localScale = scale;
    }

    internal static void SetScaleZ(this Transform tf, float z)
    {
        Vector3 scale = tf.localScale;
        scale.z = z;
        tf.localScale = scale;
    }

    internal static void SetScaleXY(this Transform tf, float x, float y)
    {
        Vector3 scale = tf.localScale;
        scale.x = x;
        scale.y = y;
        tf.localScale = scale;
    }

    internal static void SetScaleXZ(this Transform tf, float x, float z)
    {
        Vector3 scale = tf.localScale;
        scale.x = x;
        scale.z = z;
        tf.localScale = scale;
    }

    internal static void SetScaleYZ(this Transform tf, float y, float z)
    {
        Vector3 scale = tf.localScale;
        scale.y = y;
        scale.z = z;
        tf.localScale = scale;
    }

    internal static void SetScaleXYZ(this Transform tf, float x, float y, float z)
    {
        Vector3 scale = tf.localScale;
        scale.x = x;
        scale.y = y;
        scale.z = z;
        tf.localScale = scale;
    }

    internal static void SetAnchorX(this RectTransform rtf, float x)
    {
        Vector2 anchoredPos = rtf.anchoredPosition;
        anchoredPos.x = x;
        rtf.anchoredPosition = anchoredPos;
    }

    internal static void SetAnchorY(this RectTransform rtf, float y)
    {
        Vector2 anchoredPos = rtf.anchoredPosition;
        anchoredPos.y = y;
        rtf.anchoredPosition = anchoredPos;
    }

    internal static void SetWidth(this RectTransform rtf, float width)
    {
        Vector2 sizeDelta = rtf.sizeDelta;
        sizeDelta.x = width;
        rtf.sizeDelta = sizeDelta;
    }

    internal static void SetHeight(this RectTransform rtf, float height)
    {
        Vector2 sizeDelta = rtf.sizeDelta;
        sizeDelta.y = height;
        rtf.sizeDelta = sizeDelta;
    }

    internal static void SetAlpha(this Text text, float alpha)
    {
        if (text == null) return;
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    internal static void SetAlpha(this Image image, float alpha)
    {
        if (image == null) return;
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    internal static void SetAlpha(this SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }
}
