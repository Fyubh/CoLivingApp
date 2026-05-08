import SwiftUI

/// Calm secondary panel under DoorCard. DoorCard already broadcasts
/// "where I live" (unit + building), so the summary leans into "who lives
/// here" — solo / N roommates with their names. No tap action in Phase 3;
/// Phase 6 (Neighbors) and Phase 4 (Profile) own the deeper screens.
struct ApartmentSummaryCard: View {
    let context: MyApartmentContextDto

    var body: some View {
        GlassCard {
            VStack(alignment: .leading, spacing: Spacing.s12) {
                Text("Жильцы")
                    .font(.caption2.weight(.semibold))
                    .tracking(0.6)
                    .textCase(.uppercase)
                    .foregroundStyle(AppColor.inkSecondary)

                Text(headline)
                    .appText(.title)
                    .foregroundStyle(AppColor.ink)

                if !subline.isEmpty {
                    Text(subline)
                        .appText(.body)
                        .foregroundStyle(AppColor.inkSecondary)
                }
            }
        }
    }

    private var others: [RoommateDto] {
        context.roommates.filter { !$0.isMe }
    }

    private var headline: String {
        if others.isEmpty {
            return "Квартира на одного"
        }
        return Self.roommateCountLabel(others.count)
    }

    private var subline: String {
        let names = others.map(\.name)
        switch names.count {
        case 0: return ""
        case 1: return names[0]
        case 2: return "\(names[0]) и \(names[1])"
        case 3: return "\(names[0]), \(names[1]) и \(names[2])"
        default: return "\(names[0]), \(names[1]) и ещё \(names.count - 2)"
        }
    }

    /// Russian noun pluralization for "сосед". Modulo-100 in 11–14 always
    /// takes the genitive plural ("соседей"); otherwise modulo-10 picks
    /// nominative singular (1) / genitive singular (2–4) / genitive plural.
    private static func roommateCountLabel(_ n: Int) -> String {
        let mod10 = n % 10
        let mod100 = n % 100
        let noun: String
        if (11...14).contains(mod100) {
            noun = "соседей"
        } else if mod10 == 1 {
            noun = "сосед"
        } else if (2...4).contains(mod10) {
            noun = "соседа"
        } else {
            noun = "соседей"
        }
        return "\(n) \(noun)"
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(spacing: 16) {
            ApartmentSummaryCard(context: .preview(roommatesNotMe: 0))
            ApartmentSummaryCard(context: .preview(roommatesNotMe: 1))
            ApartmentSummaryCard(context: .preview(roommatesNotMe: 2))
            ApartmentSummaryCard(context: .preview(roommatesNotMe: 5))
        }
        .padding()
    }
}

private extension MyApartmentContextDto {
    static func preview(roommatesNotMe: Int) -> MyApartmentContextDto {
        let names = ["Анна", "Пётр", "Мария", "Иван", "Олег"]
        var roommates: [RoommateDto] = [
            RoommateDto(userId: "me", name: "Вы", roomNumber: "3",
                        joinedAt: Date(), isMe: true)
        ]
        for i in 0..<roommatesNotMe {
            roommates.append(RoommateDto(
                userId: "u\(i)",
                name: names[i % names.count],
                roomNumber: nil,
                joinedAt: Date(),
                isMe: false
            ))
        }
        return MyApartmentContextDto(
            apartmentId: UUID(),
            apartmentName: "Apartment 12B",
            unitNumber: "12B",
            buildingId: UUID(),
            buildingName: "The Fizz Prague",
            rooms: [],
            meUserId: "me",
            meName: "Вы",
            roommates: roommates
        )
    }
}
