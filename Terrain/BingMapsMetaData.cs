using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BingMapsMetaData
{
    public AuthenticationResultCode authenticationResultCode;
    public List<ResourceSets> resourceSets;
    public List<float> Bbox { get { return resourceSets[0].resources[0].bbox; } }
    public GeoPosition SouthWestCorner { get { return new GeoPosition(Bbox[0], Bbox[1]); } }
    public GeoPosition NorthEastCorner { get { return new GeoPosition(Bbox[2], Bbox[3]); } }
    public StatusDescription statusDescription;

    [Serializable]
    public enum AuthenticationResultCode
    {
        ValidCredentials,
        InvalidCredentials,
        CredentialsExpired,
        NotAuthorized,
        NoCredentials,
        None
    }

    [Serializable]
    public enum StatusDescription
    {
        OK = 200,
        Created = 201,
        Accepted = 202,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        InternalServerError = 500,
        ServiceUnavailable = 503
    }

    [Serializable]
    public class ResourceSets
    {
        public List<Resources> resources;
    }

    [Serializable]
    public class Resources
    {
        public List<float> bbox;
    }
}
