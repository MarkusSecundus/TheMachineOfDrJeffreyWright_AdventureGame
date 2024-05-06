using MarkusSecundus.Utils.Graphics;
using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MouseRaycast : MonoBehaviour
{
    [SerializeField] float InterpolationFactor = 0.2f;

    public static MouseRaycast Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static bool DoMouseRaycast(out Ray mouseRay, out RaycastHit info)
    {
        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(mouseRay.origin, mouseRay.direction * 3000f, Color.yellow);
        return (Physics.Raycast(mouseRay, out info, float.PositiveInfinity, LayerMasks.RoomStatic)) ;
    }

    public static Vector3 RaycastWithHeight(Ray ray, RaycastHit info, float heightAboveGround, bool useSurfaceHeightIfProvided=true)
    {
        var hitPlane = new Plane(info.normal, info.point);
        var rayOriginProjection = hitPlane.ClosestPointOnPlane(ray.origin);

        var originToOriginProjectionDistance = ray.origin.Distance(rayOriginProjection);
        var originToHitDistance = ray.origin.Distance(info.point);

        if (useSurfaceHeightIfProvided)
        {
            DragNavigatorSurface surface = info.collider.GetComponent<DragNavigatorSurface>();
            if (surface) heightAboveGround = surface.DragDistance;
        }

        var triangeSimilaritySideRatio = heightAboveGround / originToOriginProjectionDistance ;
        return info.point - (ray.direction.normalized * originToHitDistance * triangeSimilaritySideRatio);
    }


    public static bool DoMouseRaycastWithHeight(float heightAboveGround, out Vector3 point, out Ray mouseRay, out RaycastHit originalCast, out RaycastHit groundCast)
    {
        (point, mouseRay, originalCast, groundCast) = (default, default, default, default);
        if (!DoMouseRaycast(out mouseRay, out originalCast)) return false;
        point = RaycastWithHeight(mouseRay, originalCast, heightAboveGround);
        if (!RayToGround(point, originalCast, out groundCast)) return false;

        return true;
    }


    public static bool RayToGround(Vector3 heightedRaycast, RaycastHit originalInfo, out RaycastHit result)
    {
        return Physics.Raycast(new Ray(heightedRaycast, -originalInfo.normal), out result, float.PositiveInfinity, LayerMasks.RoomStatic);
    }

    [SerializeField] float aboveGround = 30f;


    public struct StateInfo
    {
        public Vector3 Point;
        public Vector3 InterpolatedPoint;
        public Ray MouseRay;
        public RaycastHit OriginalCast;
        public RaycastHit GroundCast;
        public bool IsCurrent;
    }

    bool IsFirstUpdate = true;
    public StateInfo State;


    private void FixedUpdate()
    {
        if (DoMouseRaycast(out var ray, out var info))
        {
            //var heighted = RaycastWithHeight(ray, info, aboveGround);
            //if (!RayToGround(heighted, info, out var heightedInfo)) ;// Debug.LogWarning($"Heighted raycast missed!");
            //
            //Debug.DrawRay(ray.origin, ray.direction * 3000f, Color.yellow);
            //DrawHelpers.DrawWireSphere(heighted, 10f, (a, b) => Debug.DrawLine(a, b, Color.red), circles: 10);
            //Debug.DrawLine(heighted, heightedInfo.point, Color.yellow);
            //
            //DrawHelpers.DrawWireSphere(heightedInfo.point, 10f, (a, b) => Debug.DrawLine(a, b, Color.green), circles : 10);

            if(DoMouseRaycastWithHeight(aboveGround, out var point, out var mouseRay, out var originalCast, out var groundCast))
            {
                var lastPoint = State.InterpolatedPoint;
                State = new StateInfo
                {
                    Point = point,
                    InterpolatedPoint = point,
                    MouseRay = mouseRay,
                    OriginalCast = originalCast,
                    GroundCast = groundCast,
                    IsCurrent = true
                };
                if (!IsFirstUpdate)
                {
                    State.InterpolatedPoint = Vector3.Lerp(lastPoint, State.InterpolatedPoint, InterpolationFactor);
                }
                IsFirstUpdate = false;
            }
            else
            {
                State.IsCurrent = false;
            }

        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 3000f, Color.blue);
            //Debug.Log($"No raycast hit");
        }
    }
}
