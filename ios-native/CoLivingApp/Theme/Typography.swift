import SwiftUI

/// Type scale. Hero/display use New York (system serif) for keycard-like
/// presence; body sizes stay on SF Text. `.capUpper` is now a section-header
/// style — the old uppercase rendering was retired (iOS 7-13 paradigm), but
/// the case name is preserved for source compatibility.
enum AppTextStyle {
    case hero, display, title, headline, body, bodyMed, footnote, footMed, caption, capUpper, numeric

    var font: Font {
        switch self {
        case .hero:     return .system(.largeTitle, design: .serif).weight(.semibold)
        case .display:  return .system(.title, design: .serif).weight(.semibold)
        case .title:    return .title2.weight(.semibold)
        case .headline: return .headline
        case .body:     return .subheadline
        case .bodyMed:  return .subheadline.weight(.semibold)
        case .footnote: return .footnote
        case .footMed:  return .footnote.weight(.medium)
        case .caption:  return .caption2.weight(.semibold)
        case .capUpper: return .title3.weight(.regular)
        case .numeric:  return .title2.weight(.semibold).monospacedDigit()
        }
    }

    var tracking: CGFloat {
        switch self {
        case .hero:     return -1.0
        case .display:  return -0.6
        case .title:    return -0.4
        case .headline: return -0.2
        case .body, .bodyMed: return -0.1
        case .capUpper: return -0.2
        case .footnote, .footMed, .caption, .numeric: return 0
        }
    }

    var lineSpacing: CGFloat {
        switch self {
        case .hero:     return 4
        case .display:  return 2
        case .body, .bodyMed: return 2
        default:        return 0
        }
    }
}

private struct AppTextStyleModifier: ViewModifier {
    let style: AppTextStyle
    func body(content: Content) -> some View {
        content
            .font(style.font)
            .tracking(style.tracking)
            .lineSpacing(style.lineSpacing)
    }
}

extension View {
    func appText(_ style: AppTextStyle) -> some View {
        modifier(AppTextStyleModifier(style: style))
    }
}
