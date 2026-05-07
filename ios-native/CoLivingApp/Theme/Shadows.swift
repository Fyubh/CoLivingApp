import SwiftUI

/// Two-layer depth: soft neutral ambient (large radius, low opacity) + tight
/// key (small radius, slightly tinted). One-layer drop-shadows always read
/// flat or glued — the pair gives proper z-axis presence.
enum AppShadow {
    case card, raised, fab, hero, doorCard

    fileprivate var ambient: ShadowLayer {
        switch self {
        case .card:
            return ShadowLayer(color: AppColor.ink, opacity: 0.04, radius: 30, y: 12)
        case .raised:
            return ShadowLayer(color: AppColor.ink, opacity: 0.06, radius: 50, y: 24)
        case .fab:
            return ShadowLayer(color: AppColor.ink, opacity: 0.05, radius: 36, y: 16)
        case .hero, .doorCard:
            return ShadowLayer(color: AppColor.ink, opacity: 0.06, radius: 44, y: 22)
        }
    }

    fileprivate var key: ShadowLayer {
        switch self {
        case .card:
            return ShadowLayer(color: AppColor.ink, opacity: 0.08, radius: 6, y: 2)
        case .raised:
            return ShadowLayer(color: AppColor.ink, opacity: 0.10, radius: 10, y: 4)
        case .fab:
            return ShadowLayer(color: AppColor.conifer, opacity: 0.20, radius: 12, y: 4)
        case .hero, .doorCard:
            return ShadowLayer(color: AppColor.conifer, opacity: 0.12, radius: 10, y: 4)
        }
    }
}

private struct ShadowLayer {
    let color: Color
    let opacity: Double
    let radius: CGFloat
    let y: CGFloat
}

extension View {
    func colivingShadow(_ kind: AppShadow) -> some View {
        let a = kind.ambient
        let k = kind.key
        return self
            .shadow(color: a.color.opacity(a.opacity), radius: a.radius, x: 0, y: a.y)
            .shadow(color: k.color.opacity(k.opacity), radius: k.radius, x: 0, y: k.y)
    }
}
