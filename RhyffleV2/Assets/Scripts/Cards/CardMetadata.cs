/// <summary>
/// 카드 도감 entry. v2 2차 (COSMO Objekt 데이터 모델) + v2 1차 (포커 매커닉) 통합.
/// JSON 카드 도감 DB (`Artist Season Member CollectionNo` -> CardMetadata) deserialization 타깃.
///
/// Spec 출처:
/// - 카드 시스템 v2 2차 (35d36a63...): COSMO Objekt 5 필드 + ImageFile
/// - 카드 시스템 v2 1차 (26736a63...): 포커 매커닉 (Suit/Rank/Rarity/Group/DoubleSided/UniqueAbility)
/// - 데모 spec 범위 결정 (2026-05-23): 1차 + 2차 둘 다 데모 포함
///
/// 미해결 (한울 답변 대기):
/// - Class 명칭 Premier→Special 페이지 미반영 → 일단 string. enum화는 답변 후.
/// - 등급 단계 4 vs 5 (Card DB는 5단계, UI/UX는 4단계) → CardRarity enum 기존 정의 사용.
/// </summary>
public class CardMetadata {
    // === COSMO Objekt (v2 2차) ===
    public string ImageFile;     // 파일명만 (클라가 prefix). 예: "ARTMS_Divine01_Heejin_120Z.jpg"
    public string Artist;        // 그룹명. tripleS / ARTMS / idntt (ARDEN 1차 제외)
    public string Member;        // 영문명. SeoYeon, ChaeYeon, Heejin 등
    public string Season;        // tripleS/ARTMS: Atom01~Ever01 순환 / idntt: Spring25~Spring26 순환
    public string CollectionNo;  // 숫자3자리 + A/Z (예: "101Z", "120A")
    public string Class;         // First/Double/Special/Premier 등. Premier→Special 변경 펜딩 (5/21 디코)

    // === 포커 매커닉 (v2 1차 — 데모 포함 결정) ===
    public CardSuit   Suit;
    public CardRank   Rank;
    public CardRarity Rarity;
    public CardGroup  Group;
    public bool       DoubleSided;
    public string     UniqueAbilityId;  // 도감 단위 고정. 일반 카드는 null/empty

    /// <summary>
    /// JSON 카드 도감 DB 의 composite key. 형식: "<Artist> <Season> <Member> <CollectionNo>".
    /// 예: "tripleS Atom01 ChaeYeon 101Z"
    /// </summary>
    public string Key => $"{Artist} {Season} {Member} {CollectionNo}";
}
