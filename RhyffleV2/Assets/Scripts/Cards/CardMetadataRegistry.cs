using System.Collections.Generic;

/// <summary>
/// 카드 도감 in-memory registry. JSON 카드 도감 DB 의 Dictionary&lt;string, CardMetadata&gt;
/// 형태를 코드에서 보유.
///
/// Key = CardMetadata.Key ("&lt;Artist&gt; &lt;Season&gt; &lt;Member&gt; &lt;CollectionNo&gt;").
/// CardInstance 의 ObjektName 이 이 key 를 가리킴 — GetByInstance 로 즉시 룩업.
///
/// 사용처 (예정):
/// - JsonDeckSource: JSON 로드 후 모든 CardMetadata 를 Add
/// - 점수 계산: CardInstance → CardMetadata 룩업해서 class/artist 등 접근
/// - UI: 카드 화면 표시 시 도감 정보 참조
/// </summary>
public class CardMetadataRegistry {
    private readonly Dictionary<string, CardMetadata> _byKey = new Dictionary<string, CardMetadata>();

    public int Count => _byKey.Count;

    /// <summary>도감에 추가. 동일 key 존재 시 overwrite.</summary>
    public void Add(CardMetadata metadata) {
        if (metadata == null) return;
        _byKey[metadata.Key] = metadata;
    }

    /// <summary>key 로 룩업. 없으면 null.</summary>
    public CardMetadata Get(string key) {
        if (key == null) return null;
        _byKey.TryGetValue(key, out var m);
        return m;
    }

    /// <summary>CardInstance.ObjektName 으로 룩업 helper. null instance → null.</summary>
    public CardMetadata GetByInstance(CardInstance instance) {
        if (instance == null) return null;
        return Get(instance.ObjektName);
    }
}
