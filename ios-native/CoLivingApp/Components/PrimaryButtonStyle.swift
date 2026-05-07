import SwiftUI

enum AppButtonSize {
    case sm, md, lg

    var height: CGFloat {
        switch self {
        case .sm: return 38
        case .md: return 48
        case .lg: return 54
        }
    }

    var cornerRadius: CGFloat {
        switch self {
        case .sm: return 12
        case .md: return 14
        case .lg: return 16
        }
    }

    var font: Font {
        switch self {
        case .sm: return .subheadline.weight(.semibold)
        case .md: return .body.weight(.semibold)
        case .lg: return .headline
        }
    }

    var horizontalPadding: CGFloat {
        switch self {
        case .sm: return 14
        case .md: return 18
        case .lg: return 22
        }
    }
}

/// Primary action button. Vertical pigment shift + top-edge highlight stroke
/// give the surface depth; press is a depth-press (translateY + brightness +
/// micro-scale) rather than the banal "shrink to 0.98".
struct PrimaryButtonStyle: ButtonStyle {
    var size: AppButtonSize = .lg
    var fullWidth: Bool = true

    func makeBody(configuration: Configuration) -> some View {
        let isDestructive = configuration.role == .destructive
        let topFill: Color    = isDestructive ? AppColor.danger        : AppColor.conifer
        let bottomFill: Color = isDestructive ? AppColor.dangerPressed : AppColor.coniferPressed
        let shape = RoundedRectangle(cornerRadius: size.cornerRadius, style: .continuous)

        return configuration.label
            .font(size.font)
            .foregroundStyle(.white)
            .tracking(-0.2)
            .frame(maxWidth: fullWidth ? .infinity : nil)
            .frame(height: size.height)
            .padding(.horizontal, size.horizontalPadding)
            .background(
                shape.fill(
                    LinearGradient(
                        colors: [topFill, bottomFill],
                        startPoint: .top,
                        endPoint: .bottom
                    )
                )
            )
            .overlay(
                shape.strokeBorder(
                    LinearGradient(
                        colors: [Color.white.opacity(0.18), Color.white.opacity(0.0)],
                        startPoint: .top,
                        endPoint: .bottom
                    ),
                    lineWidth: 0.6
                )
            )
            .shadow(color: AppColor.ink.opacity(configuration.isPressed ? 0.04 : 0.06),
                    radius: configuration.isPressed ? 12 : 24,
                    x: 0, y: configuration.isPressed ? 6 : 12)
            .shadow(color: topFill.opacity(configuration.isPressed ? 0.12 : 0.22),
                    radius: configuration.isPressed ? 4 : 10,
                    x: 0, y: configuration.isPressed ? 1 : 4)
            .offset(y: configuration.isPressed ? 1 : 0)
            .scaleEffect(configuration.isPressed ? 0.99 : 1)
            .brightness(configuration.isPressed ? -0.06 : 0)
            .animation(.spring(response: 0.28, dampingFraction: 0.78),
                       value: configuration.isPressed)
    }
}

/// Secondary action — glass-tinted with a low-opacity stroke. Reads like the
/// material of a bordered glass control rather than a tinted-fill chip.
struct SecondaryButtonStyle: ButtonStyle {
    var size: AppButtonSize = .lg
    var fullWidth: Bool = true

    func makeBody(configuration: Configuration) -> some View {
        let isDestructive = configuration.role == .destructive
        let tint: Color = isDestructive ? AppColor.danger : AppColor.conifer
        let shape = RoundedRectangle(cornerRadius: size.cornerRadius, style: .continuous)

        return configuration.label
            .font(size.font)
            .foregroundStyle(tint)
            .tracking(-0.2)
            .frame(maxWidth: fullWidth ? .infinity : nil)
            .frame(height: size.height)
            .padding(.horizontal, size.horizontalPadding)
            .glassEffect(.clear, in: shape)
            .overlay(
                shape.strokeBorder(tint.opacity(configuration.isPressed ? 0.40 : 0.28),
                                   lineWidth: 0.8)
            )
            .offset(y: configuration.isPressed ? 0.5 : 0)
            .brightness(configuration.isPressed ? -0.04 : 0)
            .animation(.spring(response: 0.24, dampingFraction: 0.8),
                       value: configuration.isPressed)
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(spacing: 12) {
            Button("Войти") {}
                .buttonStyle(PrimaryButtonStyle())
            Button("Сохранить") {}
                .buttonStyle(PrimaryButtonStyle(size: .md))
            Button("Отмена") {}
                .buttonStyle(SecondaryButtonStyle())
            Button("Удалить", role: .destructive) {}
                .buttonStyle(PrimaryButtonStyle())
            Button("Удалить", role: .destructive) {}
                .buttonStyle(SecondaryButtonStyle())
        }
        .padding()
    }
}
