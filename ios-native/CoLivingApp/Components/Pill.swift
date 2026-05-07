import SwiftUI

/// Status chip. Default treatment: low-saturation tinted capsule with full-tint
/// foreground. The `.dot` variant drops the background entirely — colored
/// indicator + ink text — for inline state markers.
struct Pill: View {
    enum Variant {
        case neutral, accent, success, warning, danger, info, dot

        var color: Color {
            switch self {
            case .neutral: return AppColor.inkSecondary
            case .accent:  return AppColor.conifer
            case .success: return AppColor.conifer
            case .warning: return AppColor.warning
            case .danger:  return AppColor.danger
            case .info:    return AppColor.conifer
            case .dot:     return AppColor.clay
            }
        }
    }

    private let text: String
    private let variant: Variant
    private let leading: AnyView
    private let hasLeading: Bool

    init(_ text: String, variant: Variant = .neutral, icon: String? = nil) {
        self.text = text
        self.variant = variant
        if let icon {
            self.leading = AnyView(
                Image(systemName: icon).font(.caption2.weight(.semibold))
            )
            self.hasLeading = true
        } else {
            self.leading = AnyView(EmptyView())
            self.hasLeading = false
        }
    }

    init<L: View>(_ text: String, variant: Variant = .neutral, @ViewBuilder leading: () -> L) {
        self.text = text
        self.variant = variant
        self.leading = AnyView(leading())
        self.hasLeading = true
    }

    var body: some View {
        let color = variant.color
        if variant == .dot {
            HStack(spacing: 8) {
                Circle()
                    .fill(color)
                    .frame(width: 6, height: 6)
                Text(text)
                    .font(.footnote.weight(.medium))
                    .tracking(-0.1)
            }
            .foregroundStyle(AppColor.ink)
        } else {
            HStack(spacing: hasLeading ? 6 : 0) {
                if hasLeading { leading }
                Text(text)
                    .font(.footnote.weight(.medium))
                    .tracking(-0.1)
            }
            .padding(.horizontal, 12)
            .frame(height: 28)
            .foregroundStyle(color)
            .background(Capsule().fill(color.opacity(0.10)))
        }
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(alignment: .leading, spacing: 8) {
            Pill("В процессе", variant: .accent)
            Pill("Готово", variant: .success, icon: "checkmark")
            Pill("Срочно", variant: .danger, icon: "exclamationmark.triangle.fill")
            Pill("Скоро", variant: .warning)
            Pill("Доступно", variant: .dot)
            Pill("Custom") {
                Circle().fill(AppColor.warning).frame(width: 6, height: 6)
            }
        }
        .padding()
    }
}
