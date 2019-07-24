/**************************************************************************************************
 * 
 *-------------------------------------------------------------------------------------------------
 * Copyright 2018   		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2018, Electronic Visualization Laboratory, University of Illinois at Chicago
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions 
 * and the following disclaimer. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the documentation and/or other 
 * materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES; LOSS OF 
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *************************************************************************************************/
using UnityEngine;

// This is an implementation of Generalized Perspective Projection based on a paper of the same name
// by Robert Kooima, August 2008 / revised June 2009
// http://csc.lsu.edu/~kooima/pdfs/gen-perspective.pdf
// Additional references on Unity-specific implementation
// https://forum.unity.com/threads/off-axis-projection-with-unity.192409/
public class GeneralizedPerspectiveProjection : MonoBehaviour {

    [SerializeField]
    protected Vector3 screenUL = new Vector3(-1.0215f, 2.476f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLL = new Vector3(-1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLR = new Vector3(1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    protected Transform head;

    [SerializeField]
    bool debug = false;

    protected bool useProjection = true;

    [SerializeField]
    protected Camera virtualCamera;

    Vector3 offset = Vector3.zero;
    bool applyPosition = true;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (useProjection)
        {
            Projection(screenLL, screenLR, screenUL, head.localPosition + offset, virtualCamera.nearClipPlane, virtualCamera.farClipPlane);
        }
    }

    // pa = Screen position - Lower left corner
    // pb = Screen position - Lower right corner
    // pc = Screen position - Upper left corner
    // pe = Viewer/camera position
    // n = Near clipping plane
    // f = Far clipping plane
    protected void Projection( Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pe, float n, float f)
    {
        // Non-unit vectors of screen corners
        Vector3 va, vb, vc;

        // Perpendicular unit vectors of orthonormal screen space
        // Right, Up, Normal
        Vector3 vr, vu, vn;

        float l, r, b, t;
        Matrix4x4 M = new Matrix4x4();

        // Compute an orthonormal basis for screen
        vr = pb - pa;
        vu = pc - pa;

        vr = Vector3.Normalize(vr);
        vu = Vector3.Normalize(vu);
        vn = Vector3.Cross(vr, vu).normalized;

        // Compute the screen corner vectors
        va = pa - pe;
        vb = pb - pe;
        vc = pc - pe;

        if (debug)
        {
            Debug.DrawRay(virtualCamera.transform.position, va, Color.green);
            Debug.DrawRay(virtualCamera.transform.position, vb, Color.green);
            Debug.DrawRay(virtualCamera.transform.position, vc, Color.green);
        }

        // Find the distance between the eye to screen plane
        float d = Vector3.Dot(va, vn);

        // Find the extent of the perpendicular projection
        l = Vector3.Dot(vr, va) * n / d;
        r = Vector3.Dot(vr, vb) * n / d;
        b = Vector3.Dot(vu, va) * n / d;
        t = Vector3.Dot(vu, vc) * n / d;

        //M[0] = vr[0]; M[4] = vr[1]; M[8] = vr[2];
        //M[1] = vu[0]; M[5] = vu[1]; M[9] = vu[2];
        //M[2] = vn[0]; M[6] = vn[1]; M[10] = vn[2];
        //M[15] = 1.0f;

        M[0, 0] = 2.0f * n / (r - l);
        M[0, 2] = (r + l) / (r - l);
        M[1, 1] = 2.0f * n / (t - b);
        M[1, 2] = (t + b) / (t - b);
        M[2, 2] = (f + n) / (n - f);
        M[2, 3] = 2.0f * f * n / (n - f);
        M[3, 2] = -1.0f;

        virtualCamera.projectionMatrix = M;

        if (applyPosition)
        {
            virtualCamera.transform.localPosition = pe;
        }
    }

    public void UseProjection(bool value)
    {
        useProjection = value;
    }

    public void DisablePosition()
    {
        applyPosition = false;
    }

    public void SetOffset(Vector3 viewer)
    {
        offset = viewer;
    }

    public Vector3 GetScreenUL()
    {
        return screenUL;
    }

    public Vector3 GetScreenLL()
    {
        return screenLL;
    }

    public Vector3 GetScreenLR()
    {
        return screenLR;
    }

    public void UpdateScreenUL_x(string value)
    {
        float.TryParse(value, out screenUL.x);
        BroadcastMessage("SetScreenUL", screenUL);
    }

    public void UpdateScreenUL_y(string value)
    {
        float.TryParse(value, out screenUL.y);
        BroadcastMessage("SetScreenUL", screenUL);
    }

    public void UpdateScreenUL_z(string value)
    {
        float.TryParse(value, out screenUL.z);
        BroadcastMessage("SetScreenUL", screenUL);
    }

    public void SetScreenUL(Vector3 newUL)
    {
        screenUL = newUL;
    }

    public void UpdateScreenLL_x(string value)
    {
        float.TryParse(value, out screenLL.x);
        BroadcastMessage("SetScreenLL", screenLL);
    }

    public void UpdateScreenLL_y(string value)
    {
        float.TryParse(value, out screenLL.y);
        BroadcastMessage("SetScreenLL", screenLL);
    }

    public void UpdateScreenLL_z(string value)
    {
        float.TryParse(value, out screenLL.z);
        BroadcastMessage("SetScreenLL", screenLL);
    }

    public void SetScreenLL(Vector3 newLL)
    {
        screenLL = newLL;
    }

    public void UpdateScreenLR_x(string value)
    {
        float.TryParse(value, out screenLR.x);
        BroadcastMessage("SetScreenLR", screenLR);
    }

    public void UpdateScreenLR_y(string value)
    {
        float.TryParse(value, out screenLR.y);
        BroadcastMessage("SetScreenLR", screenLR);
    }

    public void UpdateScreenLR_z(string value)
    {
        float.TryParse(value, out screenLR.z);
        BroadcastMessage("SetScreenLR", screenLR);
    }

    public void SetScreenLR(Vector3 newLR)
    {
        screenLR = newLR;
    }

    public void SetVirtualCamera(Camera c)
    {
        virtualCamera = c;
    }

    public void SetHeadTracker(Transform h)
    {
        head = h;
    }
}
