import SwiftUI

/// Hero panel — a residential keycard. Kicker microlabel sits on top, the
/// apartment number reads in serif at hero scale, a Conifer key glyph anchors
/// the right edge, and a footer status pairs a small dot with a label.
/// No chevron, no gradient: a refractive Linen mesh sits behind the glass so
/// the panel picks up warm tint instead of broadcasting brand color.
///
/// Phase 3 footer reads `Скоро` (neutral dot) until the external Keys app
/// ships — flip to `Доступно` + Clay dot once that deep link is real.
struct DoorCard: View {
    private let roomLabel: String
    private let action: () -> Void

    init(roomLabel: String, action: @escaping () -> Void = {}) {
        self.roomLabel = roomLabel
        self.action = action
    }

    var body: some View {
        let shape = RoundedRectangle(cornerRadius: Radius.hero, style: .continuous)
        let parts = Self.split(roomLabel: roomLabel)
        let kicker = Self.kicker(parts: parts)

        return Button(action: action) {
            VStack(alignment: .leading, spacing: 0) {
                if let kicker {
                    Text(kicker)
                        .font(.caption2.weight(.semibold))
                        .tracking(0.6)
                        .textCase(.uppercase)
                        .foregroundStyle(AppColor.inkSecondary)
                        .padding(.bottom, Spacing.s8)
                }

                HStack(alignment: .lastTextBaseline) {
                    Text(parts.unit)
                        .appText(.hero)
                        .foregroundStyle(AppColor.ink)
                    Spacer(minLength: 0)
                }
                .overlay(alignment: .bottomTrailing) {
                    ZStack {
                        Circle()
                            .fill(.doorGradient)
                            .frame(width: 52, height: 52)
                        Image(systemName: "key.horizontal.fill")
                            .font(.system(size: 19, weight: .semibold))
                            .foregroundStyle(.white)
                    }
                    .overlay {
                        Circle().strokeBorder(.white.opacity(0.18), lineWidth: 0.5)
                    }
                }

                HStack(spacing: 8) {
                    Circle()
                        .fill(AppColor.inkTertiary)
                        .frame(width: 6, height: 6)
                    Text("Скоро")
                        .font(.footnote.weight(.medium))
                        .foregroundStyle(AppColor.inkSecondary)
                }
                .padding(.top, Spacing.s18)
            }
            .padding(Spacing.s24)
            .frame(maxWidth: .infinity, alignment: .leading)
            .glassEffect(.regular, in: shape)
            .background {
                HeroBackdrop().clipShape(shape)
            }
            .overlay {
                shape.strokeBorder(AppColor.glassStroke, lineWidth: 0.5)
            }
            .clipShape(shape)
            .colivingShadow(.hero)
        }
        .buttonStyle(DoorCardPressStyle())
    }

    /// Pulls a digit-leading unit from the label so "Квартира 12B · Прага"
    /// becomes kicker "КВАРТИРА · ПРАГА" + hero "12B". If no digits, the whole
    /// pre-separator phrase becomes the hero text and the kicker is omitted.
    private static func split(roomLabel: String)
        -> (kicker: String?, unit: String, location: String?)
    {
        let separators: Set<Character> = ["·", "•"]
        let head: String
        let location: String?
        if let idx = roomLabel.firstIndex(where: { separators.contains($0) }) {
            head = String(roomLabel[..<idx]).trimmingCharacters(in: .whitespaces)
            let tail = String(roomLabel[roomLabel.index(after: idx)...])
                .trimmingCharacters(in: .whitespaces)
            location = tail.isEmpty ? nil : tail
        } else {
            head = roomLabel
            location = nil
        }

        if let firstDigit = head.firstIndex(where: { $0.isNumber }) {
            let kickerHead = String(head[..<firstDigit]).trimmingCharacters(in: .whitespaces)
            let unit = String(head[firstDigit...]).trimmingCharacters(in: .whitespaces)
            return (kickerHead.isEmpty ? nil : kickerHead, unit, location)
        }
        return (nil, head, location)
    }

    private static func kicker(parts: (kicker: String?, unit: String, location: String?)) -> String? {
        let pieces = [parts.kicker, parts.location].compactMap { $0 }
        return pieces.isEmpty ? nil : pieces.joined(separator: " · ")
    }
}

private struct DoorCardPressStyle: ButtonStyle {
    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .scaleEffect(configuration.isPressed ? 0.99 : 1)
            .brightness(configuration.isPressed ? -0.04 : 0)
            .animation(.spring(response: 0.45, dampingFraction: 0.85),
                       value: configuration.isPressed)
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(spacing: 16) {
            DoorCard(roomLabel: "Квартира 12B · Прага")
            DoorCard(roomLabel: "Penthouse · Karlin")
        }
        .padding()
    }
}
