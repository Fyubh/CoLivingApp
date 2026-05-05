import { useState } from 'react';
import { ScrollView, Text, View } from 'react-native';
import {
  Glass,
  GradientBackground,
  Icon,
  IconName,
  PageHeader,
  Pill,
  Segmented,
} from '../components';
import { T, TYPE } from '../theme/tokens';

const TABS = ['Чаты', 'Ивенты', 'Барахолка'] as const;
type Tab = (typeof TABS)[number];

const COPY: Record<
  Tab,
  { title: string; description: string; icon: IconName }
> = {
  Чаты: {
    title: 'Общий чат здания',
    description: 'Анонсы, клубы по интересам, AI-модерация.',
    icon: 'chat',
  },
  Ивенты: {
    title: 'Афиша ивентов',
    description: 'Йога, киновечера, бранчи на крыше.',
    icon: 'calendar',
  },
  Барахолка: {
    title: 'Барахолка',
    description: 'Купить, продать, отдать вещи между жильцами.',
    icon: 'tag',
  },
};

export function CommunityScreen() {
  const [tab, setTab] = useState<Tab>('Чаты');
  const copy = COPY[tab];

  return (
    <GradientBackground>
      <PageHeader title="Комьюнити" />
      <ScrollView
        contentContainerStyle={{
          paddingHorizontal: 16,
          paddingTop: 14,
          paddingBottom: 140,
        }}
      >
        <Segmented options={TABS} value={tab} onChange={setTab} />

        <View style={{ marginTop: 18 }}>
          <Glass padding={28} radius={24} style={{ alignItems: 'center' }}>
            <View
              style={{
                width: 64,
                height: 64,
                borderRadius: 18,
                backgroundColor: T.accentSoft,
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Icon name={copy.icon} size={30} color={T.accent} />
            </View>
            <Text
              style={[
                TYPE.title,
                { color: T.text, fontSize: 20, lineHeight: 26, marginTop: 14 },
              ]}
            >
              {copy.title}
            </Text>
            <Text
              style={[
                TYPE.body,
                {
                  color: T.text2,
                  marginTop: 14,
                  textAlign: 'center',
                  maxWidth: 280,
                },
              ]}
            >
              {copy.description}
            </Text>
            <View style={{ marginTop: 14 }}>
              <Pill color={T.accent} soft={T.accentSoft}>
                В разработке
              </Pill>
            </View>
          </Glass>
        </View>
      </ScrollView>
    </GradientBackground>
  );
}
