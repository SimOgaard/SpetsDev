float4 frag (v2f i) : SV_Target
{
    return float4(i.viewNormal, 0);
}