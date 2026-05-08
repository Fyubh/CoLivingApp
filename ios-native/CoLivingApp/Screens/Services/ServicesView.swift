import SwiftUI

/// Phase 2 placeholder. Phase 5 wires the maintenance CRUD via `.sheet` and
/// renders visual-only Bookings/VIP/PS5 sections behind a feature flag.
struct ServicesView: View {
    var body: some View {
        NavigationStack {
            TabPlaceholderBody(
                icon: "wrench.and.screwdriver",
                hint: "Заявки на обслуживание, бронирования и сервис здания."
            )
            .navigationTitle("Услуги")
        }
    }
}

#Preview {
    ServicesView()
}
