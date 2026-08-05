# 08 — Qualità del testo (anti-slop)

*Un CV o una lettera che «sanno di intelligenza artificiale» danneggiano il candidato.
Questo capitolo definisce come i testi in prosa vengono resi naturali — senza toccare i
fatti e senza trucchi.*

## 8.1 Il problema

I testi generati dai modelli hanno tic riconoscibili: lineette lunghe (—) ovunque,
elenchi puntati non richiesti, frasi tutte della stessa lunghezza e tutte bilanciate
(«tuttavia», «è importante notare»), formalismo da manuale, paragrafi simmetrici. Un
selezionatore li riconosce a colpo d'occhio; nel migliore dei casi suonano freddi, nel
peggiore squalificano la candidatura.

## 8.2 Dove si interviene (e dove no)

| Testo | Rifinitura? |
|---|---|
| **Campi-prosa**: sommario del CV, descrizioni delle esperienze, lettera, corpo email | **Sì** |
| **Campi-fatto**: nomi, aziende, date, titoli, elenco competenze, recapiti | **No, mai** — sono ricopiati dal profilo e non si toccano |

La rifinitura è un **passo separato** dalla generazione (principio del compito
ristretto: un prompt = una cosa). La generazione pensa al *cosa dire* rispettando il
profilo; la rifinitura pensa solo al *come suona*.

## 8.3 Il prompt di rifinitura (`umanizzazione`)

Deriva da un prompt di casa già collaudato per l'umanizzazione dei testi, adattato a
questo prodotto. Le sue regole:

1. **Rimuovere i marcatori tipici dell'AI**: lineette lunghe (sostituite da virgole,
   punti o riformulazioni), elenchi puntati non necessari, formule fatte («in sintesi»,
   «è importante notare che», «vale la pena sottolineare»).
2. **Ritmo naturale**: alternare frasi lunghe e corte; connettivi semplici dove servono;
   non ogni frase deve essere perfettamente bilanciata.
3. **Togliere il formalismo eccessivo**: niente disclaimer continui, niente «tuttavia»
   a ogni affermazione.
4. **Rompere la simmetria**: paragrafi di lunghezza diversa, come scrive una persona.
5. **⛔ NESSUN errore di battitura.** Alcune tecniche di umanizzazione prevedono
   micro-refusi per «credibilità»; qui sono **esplicitamente esclusi** (decisione del
   mandato): un CV o una lettera di candidatura devono essere impeccabili. La
   naturalezza si ottiene con ritmo e lessico, non con gli errori.

**Vincolo di sostanza** (cablato nel prompt): riformulare **senza aggiungere né
togliere informazioni** — niente fatti nuovi, niente enfasi che il profilo non
sostiene, niente omissioni. La rifinitura è un cambio di forma; l'anti-invenzione vale
anche qui.

Il prompt riceve la **lingua di destinazione** come parametro (i tic da correggere in
inglese non sono identici a quelli italiani: il prompt contiene le due liste).

## 8.4 Il controllo dell'utente

- Il pannello Documenti (P6) mostra **prima / dopo** per ogni campo rifinito;
  l'utente può accettare, modificare a mano o tornare alla versione non rifinita.
- La rifinitura si può disattivare (Impostazioni), ma è attiva di default: è parte
  dell'identità del prodotto.
- Regola pratica ereditata dal metodo del progetto: se il prima/dopo rivela che la
  rifinitura ha cambiato un fatto, è un difetto del prompt da correggere nel pool, non
  un caso da sistemare a mano in silenzio.

## 8.5 A monte: generare già sobrio

La miglior difesa contro lo slop è non produrlo: i prompt di generazione già impongono
prosa asciutta e in prima persona («se il profilo è scarno, il sommario è breve»).
La rifinitura è il secondo filtro, non una scusa per generare male. Se in collaudo un
tic ricorrente emerge già in generazione, si corregge **quel** prompt (e si annota nel
CHANGELOG del pool).
