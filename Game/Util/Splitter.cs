using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Util {
    public class Splitter<T> {
        private bool _queuingFirstIterator;
        private Queue<T> _unyieldedElements = new Queue<T>();
        private Predicate<T> _predicate;
        private IEnumerator<T> _inputEnumerator;

        public Splitter(IEnumerable<T> inputEnumerable, Predicate<T> predicate) {
            if (inputEnumerable == null) {
                throw new ArgumentNullException("inputEnumerable");
            }

            if (predicate == null) {
                throw new ArgumentNullException("predicate");
            }
            _inputEnumerator = inputEnumerable.GetEnumerator();
            _predicate = predicate;
        }

        public IEnumerable<T> GetMatchingElements() {
            return GetNextElement(true);
        }

        public IEnumerable<T> GetNonMatchingElements() {
            return GetNextElement(false);
        }

        private IEnumerable<T> GetNextElement(bool isMatching) {
            while (true) {
                if (_queuingFirstIterator == isMatching && _unyieldedElements.Count > 0) {
                    yield return _unyieldedElements.Dequeue();
                }
                else if (_inputEnumerator.MoveNext()) {
                    if (_predicate(_inputEnumerator.Current) == isMatching) {
                        yield return _inputEnumerator.Current;
                    }
                    else {
                        _unyieldedElements.Enqueue(_inputEnumerator.Current);
                        _queuingFirstIterator = !isMatching;
                    }
                }
                else {
                    break;
                }
            }
        }

    }
}