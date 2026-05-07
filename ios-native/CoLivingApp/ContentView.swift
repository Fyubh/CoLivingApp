import SwiftUI

struct ContentView: View {
    @Namespace private var glassMorph

    var body: some View {
        ZStack {
            AppBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: Spacing.s30) {
                    headerRow
                    DoorCard(roomLabel: "Квартира 12B · Прага")
                    notificationGroup
                    pillsRow
                    neighborsBlock
                    actionsBlock
                    Color.clear.frame(height: Spacing.s30)
                }
                .padding(.horizontal, Spacing.s24)
                .padding(.top, Spacing.s30)
            }
        }
        .preferredColorScheme(.light)
    }

    private var headerRow: some View {
        HStack(alignment: .firstTextBaseline) {
            VStack(alignment: .leading, spacing: 4) {
                Text("ЧЕТВЕРГ · 7 МАЯ")
                    .font(.caption2.weight(.semibold))
                    .tracking(0.6)
                    .foregroundStyle(AppColor.inkSecondary)
                Text("Доброе утро")
                    .appText(.display)
                    .foregroundStyle(AppColor.ink)
            }
            Spacer(minLength: Spacing.s16)
            GlassEffectContainer(spacing: 10) {
                HStack(spacing: 10) {
                    GlassIconButton(icon: "bell.fill", badge: true,
                                    glassEffectID: "bell", namespace: glassMorph)
                    GlassIconButton(icon: "person.fill",
                                    glassEffectID: "person", namespace: glassMorph)
                }
            }
        }
    }

    private var notificationGroup: some View {
        VStack(alignment: .leading, spacing: Spacing.s12) {
            SectionHeader("Сегодня")
            GlassCardGroup(spacing: Spacing.s12) {
                GlassCard {
                    VStack(alignment: .leading, spacing: Spacing.s8) {
                        HStack {
                            Pill("Уведомление", variant: .accent)
                            Spacer()
                            Text("10:00")
                                .appText(.footMed)
                                .foregroundStyle(AppColor.inkSecondary)
                        }
                        Text("Завтра — техобслуживание лифта")
                            .appText(.bodyMed)
                            .foregroundStyle(AppColor.ink)
                        Text("С 10:00 до 13:00 лифт будет недоступен. Используйте лестницу.")
                            .appText(.body)
                            .foregroundStyle(AppColor.inkSecondary)
                    }
                }
                GlassCard {
                    HStack(spacing: Spacing.s12) {
                        Avatar("Виктор Петров", size: .md)
                        VStack(alignment: .leading, spacing: 2) {
                            Text("Виктор Петров").appText(.bodyMed)
                            Text("Сосед, кв. 11C")
                                .appText(.footnote)
                                .foregroundStyle(AppColor.inkSecondary)
                        }
                        Spacer()
                        Pill("онлайн", variant: .dot)
                    }
                }
            }
        }
    }

    private var pillsRow: some View {
        VStack(alignment: .leading, spacing: Spacing.s8) {
            HStack(spacing: 8) {
                Pill("В процессе", variant: .accent)
                Pill("Готово", variant: .success, icon: "checkmark")
                Pill("Срочно", variant: .danger, icon: "exclamationmark.triangle.fill")
            }
            HStack(spacing: 8) {
                Pill("Скоро", variant: .warning)
                Pill("Доступно", variant: .dot)
                Pill("Нейтр.", variant: .neutral)
            }
        }
    }

    private var neighborsBlock: some View {
        VStack(alignment: .leading, spacing: Spacing.s12) {
            SectionHeader("Соседи", trailing: { link("все") })
            HStack(spacing: 12) {
                Avatar("Алиса", size: .md)
                Avatar("Боб", size: .md)
                Avatar("Виктор Петров", size: .md)
                Avatar("Дарья", size: .md)
                Avatar("Егор", size: .md)
            }
        }
    }

    private var actionsBlock: some View {
        VStack(spacing: Spacing.s12) {
            Button("Войти") {}
                .buttonStyle(PrimaryButtonStyle())
            Button("Сохранить") {}
                .buttonStyle(PrimaryButtonStyle(size: .md))
            Button("Отмена") {}
                .buttonStyle(SecondaryButtonStyle())
            Button("Удалить", role: .destructive) {}
                .buttonStyle(PrimaryButtonStyle())
        }
    }

    private func link(_ title: String) -> some View {
        Button(title) {}
            .font(.footnote.weight(.medium))
            .foregroundStyle(AppColor.conifer)
            .buttonStyle(.plain)
    }
}

#Preview {
    ContentView()
}
