import SwiftUI

/// Top of `ProfileView`: large avatar + name / email / role pill. Uses the
/// same GlassCard surface as ApartmentSummaryCard so the two sit next to
/// each other without visual seam.
struct IdentityCard: View {
    let me: MeDto

    var body: some View {
        GlassCard {
            HStack(alignment: .top, spacing: Spacing.s16) {
                Avatar(me.name, size: .lg)

                VStack(alignment: .leading, spacing: Spacing.s6) {
                    Text(me.name)
                        .appText(.title)
                        .foregroundStyle(AppColor.ink)
                        .lineLimit(1)

                    Text(me.email)
                        .appText(.body)
                        .foregroundStyle(AppColor.inkSecondary)
                        .lineLimit(1)
                        .truncationMode(.middle)

                    Pill(me.roleLabel, variant: .neutral)
                        .padding(.top, Spacing.s4)
                }

                Spacer(minLength: 0)
            }
        }
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(spacing: 16) {
            IdentityCard(me: MeDto(
                id: "u1",
                email: "ivan.resident@fizz.test",
                name: "Иван Петров",
                role: "Tenant",
                accessLevel: 1
            ))
            IdentityCard(me: MeDto(
                id: "u2",
                email: "very.long.email.address.for.testing@coliving-os.example",
                name: "Анастасия Скоробогатова-Иванова",
                role: "Admin",
                accessLevel: 5
            ))
        }
        .padding()
    }
}
