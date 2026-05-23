/// <summary>
/// 개별 카드 객체 — 유저가 소유한 카드 한 장.
/// JSON 개별 카드 DB row 매핑 (`카드 시스템` v2 2차).
///
/// CardMetadata (도감 entry) 는 카드 종류를 정의. CardInstance 는 그 종류의
/// 특정 사본 (objektId 고유) + 강화 상태 + 획득 시간.
/// </summary>
public class CardInstance {
    public const int MAX_ENHANCEMENT_LEVEL = 10;

    /// <summary>[Primary Key] 카드의 절대 고유 식별자.</summary>
    public int ObjektId;

    /// <summary>[Foreign Key] 카드를 소유한 유저 ID.</summary>
    public int UserId;

    /// <summary>
    /// [Foreign Key] 카드 도감의 composite key.
    /// CardMetadata.Key 형식 ("<Artist> <Season> <Member> <CollectionNo>") 와 일치.
    /// </summary>
    public string ObjektName;

    /// <summary>화음 강화 횟수. 0~10. 다른 멤버 카드로 강화.</summary>
    public int HarmonyLevel;

    /// <summary>공명 강화 횟수. 0~10. 동일 멤버 카드로 강화.</summary>
    public int ResonanceLevel;

    /// <summary>카드 획득 시간 (ISO 8601 timestamp 매핑).</summary>
    public System.DateTime AcquiredAt;
}
