import { useCallback, useEffect, useState } from 'react';
import { Text, View } from 'react-native';
import { api } from '../../api/client';
import { MyApartmentContextDto } from '../../api/types';
import { Glass, GradientBackground, PageHeader, Segmented } from '../../components';
import { useSession } from '../../state/SessionContext';
import { T, TYPE } from '../../theme/tokens';
import { ChatTab } from './ChatTab';
import { CleaningTab } from './CleaningTab';
import { FinanceTab } from './FinanceTab';
import { ProductsTab } from './ProductsTab';

const TABS = ['Финансы', 'Продукты', 'Уборка', 'Связь'] as const;
type Tab = (typeof TABS)[number];

export function NeighborsScreen() {
  const { token } = useSession();
  const [context, setContext] = useState<MyApartmentContextDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [tab, setTab] = useState<Tab>('Финансы');

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    try {
      setContext(await api.getMyContext(token));
    } catch {
      /* ignore */
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  return (
    <GradientBackground>
      <PageHeader title="Соседи" />
      {context ? (
        <>
          <View style={{ paddingHorizontal: 16, paddingTop: 14, paddingBottom: 4 }}>
            <Segmented options={TABS} value={tab} onChange={setTab} />
          </View>
          {tab === 'Финансы' && <FinanceTab context={context} />}
          {tab === 'Продукты' && <ProductsTab context={context} />}
          {tab === 'Уборка' && <CleaningTab context={context} />}
          {tab === 'Связь' && <ChatTab context={context} />}
        </>
      ) : (
        <View style={{ padding: 16 }}>
          <Glass padding={22} radius={20}>
            <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
              {loading ? 'Загружаем…' : 'Квартира ещё не назначена.'}
            </Text>
          </Glass>
        </View>
      )}
    </GradientBackground>
  );
}
