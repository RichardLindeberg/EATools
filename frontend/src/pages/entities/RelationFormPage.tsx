import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { RelationFormSchema, RelationFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import './EntityFormPage.css';

interface RelationFormPageProps {
  isEdit?: boolean;
}

export function RelationFormPage({ isEdit = false }: RelationFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);

  const { data: existingRelation, isLoading: isLoadingRelation } = useEntity(
    isEdit ? 'relations' : '',
    isEdit ? id : ''
  );

  const {
    register,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<RelationFormData>({
    resolver: zodResolver(RelationFormSchema),
    defaultValues: isEdit
      ? existingRelation || {}
      : {
          sourceEntity: '',
          targetEntity: '',
          type: '',
          direction: 'Unidirectional',
          properties: {},
          description: '',
          strength: 'Required',
          cardinality: '1:N',
        },
  });

  const watchedValues = watch();
  const [originalValues] = useState(() => deepClone(watchedValues));
  const hasChanges = hasUnsavedChanges(originalValues, watchedValues);

  useEffect(() => {
    if (existingRelation) {
      reset(existingRelation);
    }
  }, [existingRelation, reset]);

  const onSubmit = async (data: RelationFormData) => {
    try {
      let response;

      if (isEdit && id) {
        response = await apiClient.patch(`/relations/${id}`, data);
      } else {
        response = await apiClient.post('/relations', data);
      }

      if (response.data?.id) {
        navigate(`/entities/relations/${response.data.id}`);
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save relation';

      if (error?.response?.status === 422 && error?.response?.data?.errors) {
        error.response.data.errors.forEach((err: any) => {
          if (err.field) {
            setError(err.field as any, {
              type: 'server',
              message: err.message,
            });
          }
        });
      }
    }
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowDiscardModal(true);
    } else {
      navigate('/entities/relations');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/relations');
  };

  if (isEdit && isLoadingRelation) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Relationship' : 'Create Relationship'}
        subtitle={isEdit ? 'Edit entity relationship' : 'Create a new relationship'}
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Relationship'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Relationship Entities</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Source Entity"
              required
              error={errors.sourceEntity?.message}
              htmlFor="sourceEntity"
            >
              <input
                id="sourceEntity"
                type="text"
                placeholder="Enter source entity ID"
                {...register('sourceEntity')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Target Entity"
              required
              error={errors.targetEntity?.message}
              htmlFor="targetEntity"
            >
              <input
                id="targetEntity"
                type="text"
                placeholder="Enter target entity ID"
                {...register('targetEntity')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Relationship Type"
              required
              error={errors.type?.message}
              htmlFor="type"
            >
              <input
                id="type"
                type="text"
                placeholder="e.g., Owns, Integrates, Processes"
                {...register('type')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Direction"
              required
              error={errors.direction?.message}
              htmlFor="direction"
            >
              <select id="direction" {...register('direction')}>
                <option value="Unidirectional">Unidirectional</option>
                <option value="Bidirectional">Bidirectional</option>
              </select>
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the relationship..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Relationship Properties</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Strength"
              error={errors.strength?.message}
              htmlFor="strength"
            >
              <select id="strength" {...register('strength')}>
                <option value="Required">Required</option>
                <option value="Optional">Optional</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Cardinality"
              error={errors.cardinality?.message}
              htmlFor="cardinality"
            >
              <select id="cardinality" {...register('cardinality')}>
                <option value="1:1">1:1 (One to One)</option>
                <option value="1:N">1:N (One to Many)</option>
                <option value="N:N">N:N (Many to Many)</option>
              </select>
            </FormFieldWrapper>
          </div>
        </div>
      </EntityFormTemplate>

      <DiscardChangesModal
        isOpen={showDiscardModal}
        onConfirm={handleConfirmDiscard}
        onCancel={() => setShowDiscardModal(false)}
      />
    </>
  );
}
