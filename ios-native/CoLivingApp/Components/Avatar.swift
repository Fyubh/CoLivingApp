import SwiftUI

struct Avatar: View {
    enum Size {
        case sm, md, lg

        var diameter: CGFloat {
            switch self {
            case .sm: return 32
            case .md: return 40
            case .lg: return 56
            }
        }

        var fontSize: CGFloat {
            switch self {
            case .sm: return 13
            case .md: return 16
            case .lg: return 20
            }
        }
    }

    private let name: String
    private let size: Size

    init(_ name: String, size: Size = .md) {
        self.name = name
        self.size = size
    }

    var body: some View {
        let pair = Self.pair(for: name)
        Circle()
            .fill(
                LinearGradient(
                    colors: [pair.top, pair.bottom],
                    startPoint: .top,
                    endPoint: .bottom
                )
            )
            .overlay {
                Text(Self.initial(from: name))
                    .font(.system(size: size.fontSize, weight: .medium, design: .rounded))
                    .foregroundStyle(.white)
                    .tracking(-0.4)
            }
            .overlay {
                Circle().strokeBorder(.white.opacity(0.40), lineWidth: 0.5)
            }
            .frame(width: size.diameter, height: size.diameter)
    }

    private static func initial(from name: String) -> String {
        let trimmed = name.trimmingCharacters(in: .whitespaces)
        return String(trimmed.first ?? "?").uppercased()
    }

    private struct ColorPair {
        let top: Color
        let bottom: Color
    }

    /// Hand-tuned set in the brand family (mossy greens, clay warms, slate
    /// neutrals). Avoids system pastel `.gradient` autotint that reads as
    /// generic "Apple Reminders avatar".
    private static let palette: [ColorPair] = [
        ColorPair(top: AppColor.conifer,
                  bottom: AppColor.coniferPressed),
        ColorPair(top: AppColor.clay,
                  bottom: Color(red: 165/255, green:  84/255, blue:  50/255)),
        ColorPair(top: Color(red:  62/255, green:  82/255, blue: 102/255),
                  bottom: Color(red:  41/255, green:  58/255, blue:  76/255)),
        ColorPair(top: Color(red: 156/255, green: 124/255, blue:  88/255),
                  bottom: Color(red: 122/255, green:  93/255, blue:  62/255)),
        ColorPair(top: Color(red:  88/255, green: 116/255, blue:  96/255),
                  bottom: Color(red:  60/255, green:  84/255, blue:  68/255)),
        ColorPair(top: Color(red: 142/255, green:  98/255, blue: 112/255),
                  bottom: Color(red: 110/255, green:  72/255, blue:  88/255)),
        ColorPair(top: Color(red: 110/255, green: 122/255, blue:  95/255),
                  bottom: Color(red:  84/255, green:  96/255, blue:  72/255))
    ]

    private static func pair(for name: String) -> ColorPair {
        var sum: UInt32 = 0
        for u in name.unicodeScalars { sum &+= u.value }
        return palette[Int(sum) % palette.count]
    }
}

#Preview {
    ZStack {
        AppBackground()
        HStack(spacing: 16) {
            Avatar("Алиса", size: .sm)
            Avatar("Боб", size: .md)
            Avatar("Виктор Петров", size: .lg)
        }
        .padding()
    }
}
